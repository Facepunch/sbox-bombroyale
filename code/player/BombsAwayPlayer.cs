using System;
using System.Collections.Generic;
using System.Numerics;
using Sandbox;

namespace Facepunch.BombsAway;

public partial class BombsAwayPlayer : AnimatedEntity
{
	public static BombsAwayPlayer Me => Game.LocalPawn as BombsAwayPlayer;

	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }
	public Angles OriginalViewAngles { get; private set; }

	public MoveController Controller { get; private set; }
	public DamageInfo LastDamageTaken { get; private set; }

	private TimeSince TimeSinceLastKilled { get; set; }

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

	public BombsAwayPlayer() : base()
	{
		Controller = new MoveController( this )
		{
			WalkSpeed = 150f
		};
	}

	public void MakePawnOf( IClient client )
	{
		Game.AssertServer();

		client.Pawn = this;
	}

	public virtual void Respawn()
	{
		TimeSinceLastKilled = 0f;
		EnableAllCollisions = true;
		EnableDrawing = true;
		LifeState = LifeState.Alive;
		Health = 100f;
		Velocity = Vector3.Zero;

		CreateHull();

		BombsAwayGame.Entity?.MoveToSpawnpoint( this );
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
		if ( info.Attacker is BombsAwayPlayer attacker )
		{
			
		}

		LastDamageTaken = info;

		if ( LifeState == LifeState.Dead )
			return;

		base.TakeDamage( info );
		this.ProceduralHitReaction( info );
	}

	public override void OnKilled()
	{
		GameManager.Current?.OnKilled( this );

		TimeSinceLastKilled = 0f;
		EnableAllCollisions = false;
		EnableDrawing = false;
		LifeState = LifeState.Dead;

		base.OnKilled();
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
		if ( LifeState == LifeState.Dead )
		{
			if ( TimeSinceLastKilled > 5f && Game.IsServer )
			{
				Respawn();
			}
		}

		if ( LifeState == LifeState.Alive )
		{
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
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
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
