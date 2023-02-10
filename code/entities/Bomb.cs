using Sandbox;
using Sandbox.Component;
using Sandbox.Utility;
using System;
using System.Linq;

namespace Facepunch.BombRoyale;

public partial class Bomb : ModelEntity, IResettable
{
	[Net] public BombRoyalePlayer Player { get; private set; }
	[Net, Change( nameof( OnIsPlacedChanged ))] public bool IsPlaced { get; private set; }
	[Net] private TimeSince TimeSincePlaced { get; set; }
	[Net] public int Range { get; private set; }

	private TimeUntil NextBlinkTime { get; set; }
	private TimeUntil BlinkEndTime { get; set; }
	private float LifeTime { get; set; } = 4f;
	private Sound FuseSound { get; set; }
	private bool HasExploded { get; set; }

	public void Reset()
	{
		Delete();
	}

	public override void Spawn()
	{
		Transmit = TransmitType.Always;
		Scale = 0.6f;

		SetModel( "models/bomb.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		Tags.Add( "solid", "bomb" );

		base.Spawn();
	}

	public void Place( BombRoyalePlayer player )
	{
		TimeSincePlaced = 0f;

		var cellSize = 32f;
		var gridX = cellSize * (player.Position.x / cellSize).Floor();
		var gridY = cellSize * (player.Position.y / cellSize).Floor();

		if ( player.HasSuperBomb )
		{
			player.HasSuperBomb = false;
			Range = 10;
		}
		else if ( player.Disease == DiseaseType.LowRange )
		{
			Range = 1;
		}
		else
		{
			Range = player.BombRange;
		}

		Tags.Add( "placed_bomb" );
		Tags.Add( $"bomb{player.Client.NetworkIdent}" );

		SetParent( null );

		Position = new Vector3( gridX + cellSize * 0.5f, gridY + cellSize * 0.5f, player.Position.z + CollisionBounds.Size.z * 0.5f );
		IsPlaced = true;
		Player = player;
		Scale = 1f;

		var glow = Components.GetOrCreate<Glow>();
		glow.InsideObscuredColor = Color.White;
		glow.InsideColor = glow.InsideObscuredColor;
		glow.Color = Color.Transparent;

		FuseSound = Sound.FromEntity( To.Everyone, "bomb.fuse", this );
	}

	public void Pickup( BombRoyalePlayer player )
	{
		player.HoldingBomb = this;

		Tags.Clear();
		Tags.Add( "solid", "bomb" );

		SetParent( player );
		Position = player.Position + Vector3.Up * 80f + player.Rotation.Forward * 4f;
		IsPlaced = false;

		FuseSound.Stop();
	}

	[Event.Tick.Server]
	private void ServerTick()
	{
		if ( !IsPlaced || BombRoyaleGame.IsPaused ) return;

		var fraction = (1f / LifeTime) * TimeSincePlaced;
		var glow = Components.GetOrCreate<Glow>();
		glow.InsideObscuredColor = Color.Lerp( Color.White, Color.Red, fraction );
		glow.InsideColor = glow.InsideObscuredColor;
		glow.Color = Color.Transparent;

		if ( TimeSincePlaced < LifeTime ) return;

		Explode();
	}

	[Event.PreRender]
	private void ClientTick()
	{
		var tx = SceneObject.Transform;

		if ( IsPlaced )
		{
			tx.Scale = 1f + (MathF.Sin( Time.Now * 10f ) * 0.15f);

			if ( Player.IsValid() )
			{
				SceneObject.Attributes.Set( "BombColor", Player.GetTeamColor() );
			}

			if ( NextBlinkTime )
			{
				SceneObject.Attributes.Set( "Whiteness", 1f );

				if ( BlinkEndTime )
					NextBlinkTime = 1f * (1f - (TimeSincePlaced / LifeTime)).Clamp( 0.1f, 1f );
			}
			else
			{
				SceneObject.Attributes.Set( "Whiteness", 0f );
				BlinkEndTime = 0.1f;
			}
		}
		else
		{
			tx.Scale = 1f;
		}

		SceneObject.Transform = tx;
	}

	private void OnIsPlacedChanged()
	{
		if ( IsPlaced )
		{
			NextBlinkTime = LifeTime * 0.4f;
		}
	}

	[ClientRpc]
	private void DoScreenShake()
	{
		var shake = new ScreenShake.Random( 1.5f, 1f + (Range * 0.5f) );
		ScreenShake.Add( shake );
	}

	private void Explode()
	{
		if ( HasExploded ) return;

		DoScreenShake( To.Everyone );
		HasExploded = true;

		BlastInDirection( Vector3.Forward );
		BlastInDirection( Vector3.Backward );
		BlastInDirection( Vector3.Left );
		BlastInDirection( Vector3.Right );

		Sound.FromWorld( To.Everyone, "bomb.explode", Position );

		if ( Game.Random.Float() < 0.5f )
		{
			var availableBlock = All.OfType<BombableEntity>()
				.Where( e => !e.IsSpaceOccupied() )
				.Shuffle()
				.FirstOrDefault();

			if ( availableBlock.IsValid() )
			{
				var p = BombRoyale.Pickup.CreateRandom();
				p.Position = availableBlock.WorldSpaceBounds.Center;
			}
		}

		FuseSound.Stop();

		Delete();
	}

	private void BlastInDirection( Vector3 direction )
	{
		var startPosition = WorldSpaceBounds.Center;
		var cellSize = 32f;
		var totalRange = (Range * cellSize);
		var trace = Trace.Ray( startPosition, startPosition + direction * totalRange )
			.WithAnyTags( "solid", "player", "pickup", "placed_bomb" )
			.Ignore( this )
			.Run();

		var fx = Particles.Create( "particles/gameplay/bombline/bomb_explosion.vpcf" );
		fx.SetPosition( 1, trace.StartPosition );
		fx.SetPosition( 2, trace.EndPosition + trace.Direction * (cellSize * 0.5f) );
		fx.Set( "radius", 1f );

		if ( trace.Entity is BombableEntity e )
		{
			Breakables.Break( e );
			e.TrySpawnPickup();
			e.Hide();
		}
		else if ( trace.Entity is BombRoyalePlayer player )
		{
			var damage = new DamageInfo()
				.WithAttacker( this )
				.WithPosition( trace.EndPosition )
				.WithWeapon( this )
				.WithDamage( 0f )
				.WithTag( "bomb" );

			player.TakeDamage( damage );
		}
		else if ( trace.Entity is Pickup pickup )
		{
			pickup.Delete();
		}
		else if ( trace.Entity is Bomb bomb )
		{
			bomb.Explode();
		}
	}
}
