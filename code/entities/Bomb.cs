using Sandbox;
using System;

namespace Facepunch.BombsAway;

public partial class Bomb : ModelEntity
{
	private TimeSince TimeSincePlaced { get; set; }
	private bool IsPlaced { get; set; }

	public override void Spawn()
	{
		EnableAllCollisions = true;
		Transmit = TransmitType.Always;

		SetModel( "models/bomb.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		Tags.Add( "passplayers" );

		base.Spawn();
	}

	public void Place( Vector3 position )
	{
		TimeSincePlaced = 0f;

		var cellSize = 32f;
		var gridX = cellSize * (position.x / cellSize).Floor();
		var gridY = cellSize * (position.y / cellSize).Floor();

		position = new Vector3( gridX + cellSize * 0.5f, gridY + cellSize * 0.5f, position.z + CollisionBounds.Size.z * 0.5f );
		Position = position;
		IsPlaced = true;
	}

	[Event.Tick.Server]
	private void ServerTick()
	{
		if ( !IsPlaced ) return;
		if ( TimeSincePlaced < 3f ) return;

		Explode();
	}

	private void Explode()
	{
		BlastInDirection( Vector3.Forward );
		BlastInDirection( Vector3.Backward );
		BlastInDirection( Vector3.Left );
		BlastInDirection( Vector3.Right );

		Delete();
	}

	private void BlastInDirection( Vector3 direction )
	{
		var startPosition = WorldSpaceBounds.Center;
		var trace = Trace.Ray( startPosition, startPosition + direction * 200f )
			.Ignore( this )
			.Run();

		var fx = Particles.Create( "particles/bomb_path.vpcf" );
		fx.SetPosition( 0, new Vector3( 32f, 32f, 32f ) );
		fx.SetPosition( 1, trace.StartPosition );
		fx.SetPosition( 2, trace.EndPosition );

		if ( trace.Entity is BombableEntity e )
		{
			Breakables.Break( e );
			e.Delete();
		}
	}
}
