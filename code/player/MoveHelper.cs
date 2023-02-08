using Sandbox;

namespace Facepunch.BombRoyale;

public struct MoveHelper
{
	public Vector3 Position;
	public Vector3 Velocity;
	public bool HitWall;

	public float GroundBounce;
	public float WallBounce;
	public float MaxStandableAngle;
	public Trace Trace;

	public MoveHelper( Vector3 position, Vector3 velocity )  : this()
	{
		Velocity = velocity;
		Position = position;
		GroundBounce = 0f;
		WallBounce = 0f;
		MaxStandableAngle = 10f;

		Trace = SetupTrace()
			.WithoutTags( "passplayers" )
			.WithAnyTags( "solid", "playerclip", "passbullets" );
	}

	public Trace SetupTrace()
	{
		return Trace.Ray( 0f, 0f ).WorldAndEntities();
	}

	public TraceResult TraceFromTo( Vector3 start, Vector3 end )
	{
		return Trace.FromTo( start, end ).Run();
	}

	public TraceResult TraceDirection( Vector3 down )
	{
		return TraceFromTo( Position, Position + down );
	}

	public float TryMove( float timestep )
	{
		var timeLeft = timestep;
		var travelFraction = 0f;

		HitWall = false;

		using var movePlanes = new VelocityClipPlanes( Velocity );

		for ( var bump = 0; bump < movePlanes.Max; bump++ )
		{
			if ( Velocity.Length.AlmostEqual( 0.0f ) )
				break;

			var pm = TraceFromTo( Position, Position + Velocity * timeLeft );

			travelFraction += pm.Fraction;

			if ( pm.Fraction > 0.03125f )
			{
				Position = pm.EndPosition + pm.Normal * 0.01f;

				if ( pm.Fraction == 1 )
					break;

				movePlanes.StartBump( Velocity );
			}

			if ( bump == 0 && pm.Hit && pm.Normal.Angle( Vector3.Up ) >= MaxStandableAngle )
			{
				HitWall = true;
			}

			timeLeft -= timeLeft * pm.Fraction;

			if ( !movePlanes.TryAdd( pm.Normal, ref Velocity, IsFloor( pm ) ? GroundBounce : WallBounce ) )
				break;
		}

		return travelFraction;
	}

	public bool IsFloor( TraceResult trace )
	{
		if ( !trace.Hit ) return false;
		return trace.Normal.Angle( Vector3.Up ) < MaxStandableAngle;
	}

	public TraceResult TraceMove( Vector3 delta )
	{
		var tr = TraceFromTo( Position, Position + delta );
		Position = tr.EndPosition;
		return tr;
	}

	public float TryMoveWithStep( float timeDelta, float stepSize )
	{
		var startPosition = Position;
		var stepMove = this;
		var fraction = TryMove( timeDelta );

		stepMove.TraceMove( Vector3.Up * stepSize );

		var stepFraction = stepMove.TryMove( timeDelta );
		var trace = stepMove.TraceMove( Vector3.Down * stepSize );

		if ( !trace.Hit ) return fraction;

		if ( trace.Normal.Angle( Vector3.Up ) > MaxStandableAngle )
			return fraction;

		if ( startPosition.Distance( Position.WithZ( startPosition.z ) ) > startPosition.Distance( stepMove.Position.WithZ( startPosition.z ) ) )
			return fraction;

		Position = stepMove.Position;
		Velocity = stepMove.Velocity;
		HitWall = stepMove.HitWall;

		return stepFraction;
	}
}
