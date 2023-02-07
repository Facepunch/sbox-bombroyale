using Sandbox;
using System.Collections.Generic;

namespace Facepunch.BombsAway;

public partial class MoveController
{
	public float WalkSpeed { get; set; }
	public float Acceleration { get; set; } = 8f;
	public float AirAcceleration { get; set; } = 24f;
	public float GroundFriction { get; set; } = 6f;
	public float StopSpeed { get; set; } = 100f;
	public float StayOnGroundAngle { get; set; } = 270f;
	public float GroundAngle { get; set; } = 46f;
	public float StepSize { get; set; } = 28f;
	public float MaxNonJumpVelocity { get; set; } = 140f;
	public float BodyGirth { get; set; } = 16f;
	public float BodyHeight { get; set; } = 72f;
	public float EyeHeight { get; set; } = 72f;
	public float Gravity { get; set; } = 800f;
	public float AirControl { get; set; } = 48f;

	protected float SurfaceFriction { get; set; }
	protected Vector3 TraceOffset { get; set; }
	protected Vector3 PreVelocity { get; set; }
	protected Vector3 Mins { get; set; }
	protected Vector3 Maxs { get; set; }

	protected HashSet<string> Events { get; set; } = new();
	protected HashSet<string> Tags { get; set; } = new();

	public Vector3 GroundNormal { get; set; }
	public Vector3 WishVelocity { get; set; }

	public BombsAwayPlayer Player { get; private set; }

	private int StuckTries { get; set; } = 0;

	public MoveController( BombsAwayPlayer player )
	{
		Player = player;
	}

	public void ClearGroundEntity()
	{
		if ( Player.GroundEntity == null )
			return;

		Player.GroundEntity = null;
		GroundNormal = Vector3.Up;
		SurfaceFriction = 1f;
	}

	public bool HasEvent( string eventName )
	{
		if ( Events == null ) return false;
		return Events.Contains( eventName );
	}

	public bool HasTag( string tagName )
	{
		if ( Tags == null ) return false;
		return Tags.Contains( tagName );
	}

	public void AddEvent( string eventName )
	{
		if ( Events == null )
			Events = new HashSet<string>();

		if ( Events.Contains( eventName ) )
			return;

		Events.Add( eventName );
	}

	public void SetTag( string tagName )
	{
		Tags ??= new HashSet<string>();

		if ( Tags.Contains( tagName ) )
			return;

		Tags.Add( tagName );
	}

	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
			.Size( mins, maxs )
			.WithoutTags( "passplayers" )
			.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
			.Ignore( Player )
			.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0f )
	{
		return TraceBBox( start, end, Mins, Maxs, liftFeet );
	}

	public virtual BBox GetHull()
	{
		var girth = BodyGirth * 0.5f;
		var mins = new Vector3( -girth, -girth, 0 );
		var maxs = new Vector3( +girth, +girth, BodyHeight );
		return new BBox( mins, maxs );
	}


	public virtual void FrameSimulate()
	{
		Player.EyeRotation = Player.ViewAngles.ToRotation();
	}

	public virtual void Simulate()
	{
		Events?.Clear();
		Tags?.Clear();

		Player.EyeLocalPosition = Vector3.Up * Scale( EyeHeight );
		UpdateBBox();

		Player.EyeLocalPosition += TraceOffset;
		Player.EyeRotation = Player.ViewAngles.ToRotation();

		if ( CheckStuckAndFix() )
		{
			// I hope this never really happens.
			return;
		}

		PreVelocity = Player.Velocity;

		Player.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		Player.Velocity += new Vector3( 0, 0, Player.BaseVelocity.z ) * Time.Delta;
		Player.BaseVelocity = Player.BaseVelocity.WithZ( 0 );

		var startOnGround = Player.GroundEntity.IsValid();

		if ( startOnGround )
		{
			Player.Velocity = Player.Velocity.WithZ( 0 );
			ApplyFriction( GroundFriction * SurfaceFriction );
		}

		WishVelocity = new Vector3( Player.InputDirection.x, Player.InputDirection.y, 0 );

		var inSpeed = WishVelocity.Length.Clamp( 0, 1 );

		WishVelocity = WishVelocity.WithZ( 0 );
		WishVelocity = WishVelocity.Normal * inSpeed;
		WishVelocity *= GetWishSpeed();

		var stayOnGround = false;

		if ( Player.GroundEntity.IsValid() )
		{
			stayOnGround = true;
			WalkMove();
		}
		else
		{
			AirMove();
		}

		CategorizePosition( stayOnGround );

		Player.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		if ( Player.GroundEntity.IsValid() )
		{
			Player.Velocity = Player.Velocity.WithZ( 0 );
		}
	}

	private void SetBBox( Vector3 mins, Vector3 maxs )
	{
		if ( Mins == mins && Maxs == maxs )
			return;

		Mins = mins;
		Maxs = maxs;
	}

	private void UpdateBBox()
	{
		var girth = BodyGirth * 0.5f;
		var mins = Scale( new Vector3( -girth, -girth, 0 ) );
		var maxs = Scale( new Vector3( +girth, +girth, BodyHeight ) );
		SetBBox( mins, maxs );
	}

	private float GetWishSpeed()
	{
		return Scale( WalkSpeed );
	}

	private void WalkMove()
	{
		var wishDir = WishVelocity.Normal;
		var wishSpeed = WishVelocity.Length;

		WishVelocity = WishVelocity.WithZ( 0 );
		WishVelocity = WishVelocity.Normal * wishSpeed;

		Player.Velocity = Player.Velocity.WithZ( 0 );

		Accelerate( wishDir, wishSpeed, 0f, Acceleration );

		Player.Velocity = Player.Velocity.WithZ( 0 );
		Player.Velocity += Player.BaseVelocity;

		try
		{
			if ( Player.Velocity.Length < 1f )
			{
				Player.Velocity = Vector3.Zero;
				return;
			}

			var dest = (Player.Position + Player.Velocity * Time.Delta).WithZ( Player.Position.z );
			var pm = TraceBBox( Player.Position, dest );

			if ( pm.Fraction == 1 )
			{
				Player.Position = pm.EndPosition;
				StayOnGround();
				return;
			}

			StepMove();
		}
		finally
		{
			Player.Velocity -= Player.BaseVelocity;
		}

		StayOnGround();
	}

	private void StepMove()
	{
		var mover = new MoveHelper( Player.Position, Player.Velocity );

		mover.Trace = mover.SetupTrace()
			.WithoutTags( "passplayers" )
			.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
			.Size( Mins, Maxs )
			.Ignore( Player );

		mover.MaxStandableAngle = GroundAngle;
		mover.TryMoveWithStep( Time.Delta, StepSize );

		Player.Position = mover.Position;
		Player.Velocity = mover.Velocity;
	}

	private void Move()
	{
		var mover = new MoveHelper( Player.Position, Player.Velocity );

		mover.Trace = mover.SetupTrace()
			.WithoutTags( "passplayers" )
			.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
			.Size( Mins, Maxs )
			.Ignore( Player );

		mover.MaxStandableAngle = GroundAngle;
		mover.TryMove( Time.Delta );

		Player.Position = mover.Position;
		Player.Velocity = mover.Velocity;
	}

	private void Accelerate( Vector3 wishDir, float wishSpeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishSpeed > speedLimit )
			wishSpeed = speedLimit;

		var currentSpeed = Player.Velocity.Dot( wishDir );
		var addSpeed = wishSpeed - currentSpeed;

		if ( addSpeed <= 0 )
			return;

		var accelSpeed = acceleration * Time.Delta * wishSpeed * SurfaceFriction;

		if ( accelSpeed > addSpeed )
			accelSpeed = addSpeed;

		Player.Velocity += wishDir * accelSpeed;
	}

	private bool CheckStuckAndFix()
	{
		var result = TraceBBox( Player.Position, Player.Position );

		if ( !result.StartedSolid )
		{
			StuckTries = 0;
			return false;
		}

		if ( Game.IsClient ) return true;

		var attemptsPerTick = 20;

		for ( int i = 0; i < attemptsPerTick; i++ )
		{
			var pos = Player.Position + Vector3.Random.Normal * (StuckTries / 2.0f);

			if ( i == 0 )
			{
				pos = Player.Position + Vector3.Up * 5;
			}

			result = TraceBBox( pos, pos );

			if ( !result.StartedSolid )
			{
				Player.Position = pos;
				return false;
			}
		}

		StuckTries++;
		return true;
	}

	private void ApplyFriction( float frictionAmount = 1f )
	{
		var speed = Player.Velocity.Length;
		if ( speed < 0.1f ) return;

		var control = (speed < StopSpeed) ? StopSpeed : speed;
		var drop = control * Time.Delta * frictionAmount;
		var newSpeed = speed - drop;

		if ( newSpeed < 0 ) newSpeed = 0;

		if ( newSpeed != speed )
		{
			newSpeed /= speed;
			Player.Velocity *= newSpeed;
		}
	}

	private float Scale( float speed )
	{
		return speed * Player.Scale;
	}

	private Vector3 Scale( Vector3 velocity )
	{
		return velocity * Player.Scale;
	}

	private void AirMove()
	{
		var wishDir = WishVelocity.Normal;
		var wishSpeed = WishVelocity.Length;

		Accelerate( wishDir, wishSpeed, AirControl, AirAcceleration );

		Player.Velocity += Player.BaseVelocity;

		Move();

		Player.Velocity -= Player.BaseVelocity;
	}

	private void CategorizePosition( bool stayOnGround )
	{
		SurfaceFriction = 1f;

		var point = Player.Position - Vector3.Up * 2f;
		var bumpOrigin = Player.Position;
		var moveToEndPos = false;

		if ( Player.GroundEntity.IsValid() )
		{
			moveToEndPos = true;
			point.z -= StepSize;
		}
		else if ( stayOnGround )
		{
			moveToEndPos = true;
			point.z -= StepSize;
		}

		if ( Player.Velocity.z > MaxNonJumpVelocity )
		{
			ClearGroundEntity();
			return;
		}

		var pm = TraceBBox( bumpOrigin, point, 16f );

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > StayOnGroundAngle )
		{
			ClearGroundEntity();
			moveToEndPos = false;

			if ( Player.Velocity.z > 0 )
				SurfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundEntity( pm );
		}

		if ( moveToEndPos && !pm.StartedSolid && pm.Fraction > 0f && pm.Fraction < 1f )
		{
			Player.Position = pm.EndPosition;
		}
	}

	private void UpdateGroundEntity( TraceResult trace )
	{
		Player.GroundEntity = trace.Entity;
		SurfaceFriction = trace.Surface.Friction * 1.25f;
		GroundNormal = trace.Normal;

		if ( SurfaceFriction > 1f )
			SurfaceFriction = 1f;

		if ( Player.GroundEntity.IsValid() )
		{
			Player.BaseVelocity = Player.GroundEntity.Velocity;
		}
	}

	private void StayOnGround()
	{
		var start = Player.Position + Vector3.Up * 2;
		var end = Player.Position + Vector3.Down * StepSize;

		var trace = TraceBBox( Player.Position, start );
		start = trace.EndPosition;

		trace = TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return;
		if ( trace.Fraction >= 1 ) return;
		if ( trace.StartedSolid ) return;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > StayOnGroundAngle ) return;

		Player.Position = trace.EndPosition;
	}
}
