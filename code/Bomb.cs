using System;
using System.Diagnostics;
using System.Linq;
using Sandbox;

namespace Facepunch.BombRoyale;

[Title( "Bomb" )]
[Category( "Bomb Royale" )]
public class Bomb : Component, IResettable
{
	[Sync] private Guid PlacerId { get; set; }
	public Player Player => Scene.Directory.FindComponentByGuid( PlacerId ) as Player;
	
	[Sync] public bool IsPlaced { get; private set; }
	[Sync] private TimeSince TimeSincePlaced { get; set; }
	[Sync] public int Range { get; private set; }
	
	[Property] public ModelRenderer Renderer { get; set; }

	private TimeUntil NextBlinkTime { get; set; }
	private TimeUntil BlinkEndTime { get; set; }
	private float LifeTime { get; set; } = 4f;
	private SoundHandle FuseSound { get; set; }
	private bool HasExploded { get; set; }

	protected override void OnStart()
	{
		Tags.Add( "solid" );
		Tags.Add( "bomb" );
		
		base.OnStart();
	}
	
	public void Place( Player player )
	{
		if ( !Networking.IsHost )
		{
			throw new( "Only the host can place a bomb" );
		}
		
		TimeSincePlaced = 0f;

		var cellSize = 32f;
		var gridX = cellSize * (player.Transform.Position.x / cellSize).Floor();
		var gridY = cellSize * (player.Transform.Position.y / cellSize).Floor();

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
		Tags.Add( $"bomb{player.PlayerSlot}" );

		GameObject.Parent = null;
		
		var collisionSize = Renderer.Bounds;

		Transform.Position = new( gridX + cellSize * 0.5f, gridY + cellSize * 0.5f, player.Transform.Position.z + collisionSize.Size.z * 0.5f );
		Transform.Scale = 1f;
		
		IsPlaced = true;
		PlacerId = player.Id;

		var glow = Components.GetOrCreate<HighlightOutline>();
		glow.InsideObscuredColor = Color.White;
		glow.InsideColor = glow.InsideObscuredColor;
		glow.Color = Color.Transparent;

		StartFuseSound( Transform.Position );
	}

	public void Pickup( Player player )
	{
		if ( !Networking.IsHost )
		{
			throw new( "Only the host can pickup a bomb" );
		}

		player.SetHoldingBomb( this );

		Tags.RemoveAll();
		Tags.Add( "solid" );
		Tags.Add( "bomb" );

		GameObject.SetParent( player.GameObject );
		Transform.Position = player.Transform.Position + Vector3.Up * 80f + player.Transform.Rotation.Forward * 4f;
		IsPlaced = false;

		StopFuseSound();
	}

	[Broadcast]
	private void StartFuseSound( Vector3 position )
	{
		FuseSound?.Stop();
		FuseSound = Sound.Play( "bomb.fuse", position );
	}

	private void StopFuseSound()
	{
		FuseSound?.Stop();
		FuseSound = null;
	}

	protected override void OnFixedUpdate()
	{
		if ( Networking.IsHost )
		{
			CheckInsideBomb();
		}

		Tick();
		
		base.OnFixedUpdate();
	}

	private void CheckInsideBomb()
	{
		if ( !IsPlaced || BombRoyale.Instance.IsPaused ) return;
		if ( !Player.IsValid() ) return;
		if ( Player.LifeState != LifeState.Alive ) return;
		if ( Player.IsInsideBomb( Player.Transform.Position ) ) return;

		Tags.Remove( $"bomb{Player.PlayerSlot}" );
	}

	private void Tick()
	{
		if ( !IsPlaced || BombRoyale.Instance.IsPaused ) return;

		var fraction = (1f / LifeTime) * TimeSincePlaced;
		var glow = Components.GetOrCreate<HighlightOutline>();
		glow.InsideObscuredColor = Color.Lerp( Color.White, Color.Red, fraction );
		glow.InsideColor = glow.InsideObscuredColor;
		glow.Color = Color.Transparent;

		if ( !Networking.IsHost ) return;
		if ( TimeSincePlaced < LifeTime ) return;

		Explode();
	}

	protected override void OnPreRender()
	{
		UpdateSceneObject();
		base.OnPreRender();
	}

	private void UpdateSceneObject()
	{
		var sceneObject = Renderer.SceneObject;
		
		if ( !sceneObject.IsValid() || !Player.IsValid() )
			return;

		var tx = sceneObject.Transform;

		if ( IsPlaced )
		{
			tx.Scale = 1f + (MathF.Sin( Time.Now * 10f ) * 0.15f);

			if ( Player.IsValid() )
			{
				sceneObject.Attributes.Set( "BombColor", Player.GetTeamColor() );
			}

			if ( NextBlinkTime )
			{
				sceneObject.Attributes.Set( "Whiteness", 1f );

				if ( BlinkEndTime )
					NextBlinkTime = 1f * (1f - (TimeSincePlaced / LifeTime)).Clamp( 0.1f, 1f );
			}
			else
			{
				sceneObject.Attributes.Set( "Whiteness", 0f );
				BlinkEndTime = 0.1f;
			}
		}
		else
		{
			tx.Scale = 1f;
		}

		sceneObject.Transform = tx;
	}

	private void ShortenFuse( float time )
	{
		if ( TimeSincePlaced < LifeTime - time )
		{
			TimeSincePlaced = LifeTime - time;
		}
	}

	private void OnIsPlacedChanged()
	{
		if ( IsPlaced )
		{
			NextBlinkTime = LifeTime * 0.4f;
		}
	}

	[Broadcast]
	private void DoScreenShake()
	{
		var shake = new ScreenShake.Random( 1.5f, 1f + (Range * 0.5f) );
		ScreenShake.Add( shake );
	}

	[Broadcast]
	private void PlayExplodeSound( Vector3 position )
	{
		Sound.Play( "bomb.explode", position );
	}

	private void Explode()
	{
		if ( !Networking.IsHost )
			throw new( "Only the host can explode a bomb" );
		
		if ( HasExploded ) return;

		DoScreenShake();
		HasExploded = true;

		BlastInDirection( Vector3.Forward );
		BlastInDirection( Vector3.Backward );
		BlastInDirection( Vector3.Left );
		BlastInDirection( Vector3.Right );

		PlayExplodeSound( Transform.Position );

		if ( Game.Random.Float() < 0.5f )
		{
			var availableBlock = Scene.GetAllComponents<Bombable>()
				.Where( e => !e.IsSpaceOccupied() )
				.Shuffle()
				.FirstOrDefault();

			if ( availableBlock.IsValid() )
			{
				/*
				var p = BombRoyale.Pickup.CreateRandom();
				p.Position = availableBlock.WorldSpaceBounds.Center;
				*/
			}
		}

		StopFuseSound();
		GameObject.Destroy();
	}

	[Broadcast]
	private void CreateBombParticles( Vector3 startPosition, Vector3 endPosition )
	{
		var fx = new SceneParticles( Scene.SceneWorld, "particles/gameplay/bombline/bomb_explosion.vpcf" );
		fx.SetControlPoint( 1, startPosition );
		fx.SetControlPoint( 2, endPosition );
		fx.SetNamedValue( "radius", 1f );
		fx.PlayUntilFinished( Task );
	}

	private void BlastInDirection( Vector3 direction )
	{
		var startPosition = Renderer.Bounds.Center;
		var cellSize = 32f;
		var totalRange = (Range * cellSize);
		var trace = Scene.Trace.Ray( startPosition, startPosition + direction * totalRange )
			.WithAnyTags( "solid", "player", "pickup", "placed_bomb" )
			.IgnoreGameObject( GameObject )
			.Run();

		CreateBombParticles( trace.StartPosition, trace.EndPosition + trace.Direction * (cellSize * 0.5f) );

		var hitObject = trace.GameObject;
		if ( !hitObject.IsValid() ) return;
		
		if ( hitObject.Components.TryGet<Bombable>( out var bombable ) )
		{
			//Breakables.Break( e );
			bombable.TrySpawnPickup();
			bombable.Hide();
		}
		else if ( hitObject.Components.TryGet<Player>( out var player ) )
		{
			player.TakeDamage( DamageType.Explosion, 0f, trace.EndPosition, Vector3.Zero, Id );
		}
		/*
		else if ( hitObject.Components.TryGet<Pickup>( out var pickup ) )
		{
			pickup.Delete();
		}
		*/
		else if ( hitObject.Components.TryGet<Bomb>( out var bomb ) )
		{
			var fuseDelay = Game.Random.Float( 0.15f, 0.3f );
			bomb.ShortenFuse( fuseDelay );
		}
	}
}
