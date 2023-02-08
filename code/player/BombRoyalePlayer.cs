using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Sandbox;
using Sandbox.Utility;

namespace Facepunch.BombRoyale;

public partial class BombRoyalePlayer : AnimatedEntity
{
	public static BombRoyalePlayer Me => Game.LocalPawn as BombRoyalePlayer;

	[Net] public Bomb HoldingBomb { get; set; }
	[Net] public bool HasSuperBomb { get; set; }
	[Net] public int SpeedBoosts { get; set; }
	[Net] public int LivesLeft { get; set; }
	[Net] public int BombRange { get; set; }
	[Net] public int MaxBombs { get; set; }

	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }
	public Angles OriginalViewAngles { get; private set; }

	public ClothingContainer Clothing { get; private set; } = new();
	public MoveController Controller { get; private set; }
	public DamageInfo LastDamageTaken { get; private set; }

	private TimeSince LastTakeDamageTime { get; set; }

	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	[Net, Predicted]
	public Vector3 EyeLocalPosition { get; set; }

	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	[Net, Predicted]
	public Rotation EyeLocalRotation { get; set; }

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	public BombRoyalePlayer() : base()
	{
		Controller = new MoveController( this )
		{
			WalkSpeed = 150f
		};
	}

	public void MakePawnOf( IClient client )
	{
		Game.AssertServer();

		Clothing.LoadFromClient( client );

		client.Pawn = this;
	}

	public int GetPlacedBombCount()
	{
		return All.OfType<Bomb>().Count( b => b.Player == this && b.IsPlaced );
	}

	public virtual void Respawn()
	{
		Particles.Create( "particles/gameplay/player/respawn/respawn_effect.vpcf", this );

		EnableAllCollisions = true;
		EnableDrawing = true;
		LifeState = LifeState.Alive;
		SpeedBoosts = 0;
		LivesLeft = 1;
		BombRange = 2;
		MaxBombs = 1; 
		Health = 100f;
		Velocity = Vector3.Zero;

		Clothing.DressEntity( this );

		CreateHull();

		BombRoyaleGame.Entity?.MoveToSpawnpoint( this );
		ResetInterpolation();
	}

	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		EnableLagCompensation = true;
		Tags.Add( "player" );

		base.Spawn();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
	}

	private TimeSince TimeSinceLastFootstep { get; set; }
	public override void OnAnimEventFootstep( Vector3 position, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !Game.IsClient )
			return;

		if ( TimeSinceLastFootstep < 0.2f )
			return;

		volume *= GetFootstepVolume();

		TimeSinceLastFootstep = 0f;

		var footletter = foot == 0 ? "l" : "r";
		var particle = Particles.Create( $"particles/gameplay/player/footsteps/footstep_{footletter}.vpcf", position );
		particle.SetOrientation( 0, Transform.Rotation );

		var tr = Trace.Ray( position, position + Vector3.Down * 20f )
			.WithoutTags( "trigger" )
			.Radius( 1f )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume );
	}

	public override void BuildInput()
	{
		OriginalViewAngles = ViewAngles;
		InputDirection = SnapInputDirection( Input.AnalogMove );

		if ( Input.StopProcessing )
			return;

		if ( InputDirection.Length > 0f )
		{
			var rotation = Rotation.LookAt( InputDirection, Vector3.Up );
			ViewAngles = rotation.Angles();
		}
	}

	public override void StartTouch( Entity other )
	{

		base.StartTouch( other );
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( LifeState == LifeState.Dead ) return;

		if ( info.Attacker is BombRoyalePlayer attacker )
		{
			
		}

		if ( info.Tags.Contains( "bomb" ) )
		{
			LastTakeDamageTime = 0f;
			LivesLeft--;

			if ( LivesLeft <= 0 )
			{
				var direction = Vector3.Up + new Vector3( Game.Random.Float( -0.25f, 0.25f ), Game.Random.Float( -0.25f, 0.25f ), 0f );
				BecomeRagdollOnClient( To.Everyone, direction * 100f * 10f, 0 );
				EnableAllCollisions = false;
				EnableDrawing = false;
				LifeState = LifeState.Dead;
			}
		}

		LastDamageTaken = info;

		this.ProceduralHitReaction( info );
	}

	public override void FrameSimulate( IClient cl )
	{
		if ( LifeState == LifeState.Alive )
		{
			Controller?.FrameSimulate();
		}
	}

	public override void Simulate( IClient client )
	{
		if ( LifeState == LifeState.Alive )
		{
			if ( Game.IsServer && Input.Released( InputButton.PrimaryAttack ) )
			{
				if ( !Controller.IsInsideBomb( Position ) )
				{
					if ( HoldingBomb.IsValid() )
					{
						// TODO: Throw bomb.
						HoldingBomb.Place( this );
						HoldingBomb = null;
					}
					else if ( GetPlacedBombCount() < MaxBombs )
					{
						var bomb = new Bomb();
						bomb.Place( this );
					}
				}
			}

			Controller?.Simulate();
			SimulateAnimation();
		}
	}

	protected virtual float GetFootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0f, 200f ) * 0.5f;
	}

	protected virtual void CreateHull()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16f, -16f, 0f ), new Vector3( 16f, 16f, 72f ) );
		EnableHitboxes = true;
	}

	[Event.Tick.Server]
	protected virtual void ServerTick()
	{
		if ( LifeState == LifeState.Dead )
		{
			return;
		}

		var tx = Transform;

		if ( LastTakeDamageTime < 2f )
		{
			var fraction = 1f - (LastTakeDamageTime / 2f);
			tx.Scale = 1f + (0.2f * MathF.Sin( Time.Now * 20f )) * fraction;
		}
		else
		{
			tx.Scale = tx.Scale.LerpTo( 1f, Time.Delta * 4f );
		}

		Transform = tx;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	[ClientRpc]
	private void BecomeRagdollOnClient( Vector3 force, int forceBone )
	{
		var ragdoll = new PlayerCorpse
		{
			Position = Position,
			Rotation = Rotation
		};

		ragdoll.CopyFrom( this );
		ragdoll.ApplyForceToBone( force, forceBone );
	}

	private Vector3[] Cardinals = new Vector3[]
	{
		Vector3.Forward,
		Vector3.Left,
		Vector3.Right,
		Vector3.Backward
	};

	private Vector3 SnapInputDirection( Vector3 direction )
	{
		if ( direction.Length == 0f )
			return direction;

		var output = Vector3.Zero;
		var ldp = -1f;

		foreach ( var c in Cardinals )
		{
			var dp = direction.Dot( c );
			if ( dp > ldp )
			{
				output = c;
				ldp = dp;
			}
		}

		return output;
	}
}
