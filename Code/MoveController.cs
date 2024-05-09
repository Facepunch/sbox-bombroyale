using Sandbox;

namespace Facepunch.BombRoyale;

[Title( "Move Controller" )]
[Category( "Bomb Royale" )]
[Icon( "directions_walk" )]
public class MoveController : Component
{
	[Range( 0f, 200f )] [Property] public float Radius { get; set; } = 16f;
	[Range( 0f, 200f )] [Property] public float Height { get; set; } = 64f;
	[Range( 0f, 50f )] [Property] public float StepHeight { get; set; } = 18f;
	[Range( 0f, 90f )] [Property] public float GroundAngle { get; set; } = 45f;
	[Range( 0f, 64f )] [Property] public float Acceleration { get; set; } = 10f;

	/// <summary>
	/// When jumping into walls, should we bounce off or just stop dead?
	/// </summary>
	[Range( 0, 1 )] [Property] public float Bounciness { get; set; } = 0.3f;
	[Property] public TagSet IgnoreLayers { get; set; } = new();
	[Property] public bool EnableFixUnstuck { get; set; } = true;

	public BBox BoundingBox => new( new( -Radius, -Radius, 0 ), new Vector3( Radius, Radius, Height ) );

	[Sync] public Vector3 Velocity { get; set; }
	[Sync] public bool IsOnGround { get; set; }

	public GameObject GroundObject { get; set; }
	public Collider GroundCollider { get; set; }
	
	private int StuckTries;
	
	protected override void DrawGizmos()
	{
		Gizmo.Draw.LineBBox( BoundingBox );
	}

	/// <summary>
	/// Add acceleration to the current velocity. No need to scale by time delta - it will be done inside.
	/// </summary>
	public void Accelerate( Vector3 vector )
	{
		Velocity = Velocity.WithAcceleration( vector, Acceleration * Time.Delta );
	}

	/// <summary>
	/// Apply an amount of friction to the current velocity. No need to scale by time delta - it will be done inside.
	/// </summary>
	public void ApplyFriction( float frictionAmount, float stopSpeed = 140f )
	{
		var speed = Velocity.Length;
		if ( speed < 0.01f ) return;

		// Bleed off some speed, but if we have less than the bleed threshold, bleed the threshold amount.
		var control = (speed < stopSpeed) ? stopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;
		
		float newSpeed = speed - drop;
		if ( newSpeed < 0 ) newSpeed = 0;
		if ( newSpeed == speed ) return;

		newSpeed /= speed;
		Velocity *= newSpeed;
	}

	SceneTrace BuildTrace( Vector3 from, Vector3 to ) => BuildTrace( Scene.Trace.Ray( from, to ) );
	SceneTrace BuildTrace( SceneTrace source ) => source.Size( BoundingBox )
		.WithoutTags( IgnoreLayers )
		.IgnoreGameObjectHierarchy( GameObject );

	/// <summary>
	/// Trace the controller's current position to the specified delta.
	/// </summary>
	public SceneTraceResult TraceDirection( Vector3 direction )
	{
		return BuildTrace( GameObject.Transform.Position, GameObject.Transform.Position + direction ).Run();
	}
	
	private void Move( bool step )
	{
		if ( step && IsOnGround )
		{
			Velocity = Velocity.WithZ( 0f );
		}

		if ( Velocity.Length < 0.001f )
		{
			Velocity = Vector3.Zero;
			return;
		}

		var pos = GameObject.Transform.Position;

		var mover = new CharacterControllerHelper( BuildTrace( pos, pos ), pos, Velocity )
		{
			Bounce = Bounciness,
			MaxStandableAngle = GroundAngle
		};

		if ( step && IsOnGround )
		{
			mover.TryMoveWithStep( Time.Delta, StepHeight );
		}
		else
		{
			mover.TryMove( Time.Delta );
		}

		Transform.Position = mover.Position;
		Velocity = mover.Velocity;
	}

	private void CategorizePosition()
	{
		var Position = Transform.Position;
		var point = Position + Vector3.Down * 2f;
		var vBumpOrigin = Position;
		var wasOnGround = IsOnGround;

		// We're flying upwards too fast, never land on ground.
		if ( !IsOnGround && Velocity.z > 40f )
		{
			ClearGround();
			return;
		}

		//
		// Trace down one step height if we're already on the ground "step down". If not, search for floor right below us
		// because if we do StepHeight we'll snap that many units to the ground.
		//
		point.z -= wasOnGround ? StepHeight : 0.1f;
		
		var pm = BuildTrace( vBumpOrigin, point ).Run();
		
		if ( !pm.Hit || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGround();
			return;
		}
		
		IsOnGround = true;
		GroundObject = pm.GameObject;
		GroundCollider = pm.Shape?.Collider as Collider;
		
		if ( wasOnGround && pm is { StartedSolid: false, Fraction: > 0f and < 1f } )
		{
			Transform.Position = pm.EndPosition + pm.Normal * 0.01f;
		}
	}

	/// <summary>
	/// Disconnect from ground and punch our velocity. This is useful if you want the player to jump or something.
	/// </summary>
	public void Punch( in Vector3 amount )
	{
		ClearGround();
		Velocity += amount;
	}

	private void ClearGround()
	{
		IsOnGround = false;
		GroundObject = default;
		GroundCollider = default;
	}

	/// <summary>
	/// Move a character with this velocity.
	/// </summary>
	public void Move()
	{
		if ( EnableFixUnstuck && TryUnstuck() )
			return;

		Move( IsOnGround );
		CategorizePosition();
	}

	/// <summary>
	/// Move from our current position to this target position, but using tracing an sliding.
	/// This is good for different control modes like ladders and stuff.
	/// </summary>
	public void MoveTo( Vector3 targetPosition, bool useStep )
	{
		if ( EnableFixUnstuck && TryUnstuck() )
			return;

		var pos = Transform.Position;
		var delta = targetPosition - pos;

		var mover = new CharacterControllerHelper( BuildTrace( pos, pos ), pos, delta );
		mover.MaxStandableAngle = GroundAngle;

		if ( useStep )
			mover.TryMoveWithStep( 1f, StepHeight );
		else
			mover.TryMove( 1f );

		Transform.Position = mover.Position;
	}

	private bool TryUnstuck()
	{
		var result = BuildTrace( Transform.Position, Transform.Position ).Run();
		
		if ( !result.StartedSolid )
		{
			StuckTries = 0;
			return false;
		}

		var attemptsPerTick = 20;

		for ( int i = 0; i < attemptsPerTick; i++ )
		{
			var pos = Transform.Position + Vector3.Random.Normal * (((float)StuckTries) / 2f);

			if ( i == 0 )
				pos = Transform.Position + Vector3.Up * 2f;

			result = BuildTrace( pos, pos ).Run();

			if ( result.StartedSolid )
				continue;
			
			Transform.Position = pos;
			return false;
		}

		StuckTries++;
		return true;
	}
}
