using Sandbox;
using System;

namespace Facepunch.BombsAway;

public partial class Bomb : ModelEntity
{
	public BombsAwayPlayer Player { get; private set; }

	private TimeSince TimeSincePlaced { get; set; }
	private bool IsPlaced { get; set; }

	public override void Spawn()
	{
		EnableAllCollisions = true;
		Transmit = TransmitType.Always;
		Scale = 0.6f;

		SetModel( "models/bomb.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		Tags.Add( "passplayers" );

		base.Spawn();
	}

	public void Place( BombsAwayPlayer player )
	{
		TimeSincePlaced = 0f;

		var cellSize = 32f;
		var gridX = cellSize * (player.Position.x / cellSize).Floor();
		var gridY = cellSize * (player.Position.y / cellSize).Floor();

		SetParent( null );

		Position = new Vector3( gridX + cellSize * 0.5f, gridY + cellSize * 0.5f, player.Position.z + CollisionBounds.Size.z * 0.5f );
		IsPlaced = true;
		Player = player;
		Scale = 1f;
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
		var cellSize = 32f;
		var totalRange = (Player.BombRange * cellSize) + (cellSize * 0.5f);
		var trace = Trace.Ray( startPosition, startPosition + direction * totalRange )
			.WithAnyTags( "solid", "player" )
			.Ignore( this )
			.Run();

		var fx = Particles.Create( "particles/bomb_path.vpcf" );
		fx.SetPosition( 1, trace.StartPosition );
		fx.SetPosition( 2, trace.EndPosition );
		fx.Set( "radius", 1f );

		if ( trace.Entity is BombableEntity e )
		{
			Breakables.Break( e );
			e.Delete();
		}
	}
}
