using Sandbox;

namespace Facepunch.BombsAway;

public partial class Bomb : ModelEntity
{
	public override void Spawn()
	{
		EnableAllCollisions = true;
		Transmit = TransmitType.Always;

		SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero, 10f );

		base.Spawn();
	}

	[Event.Tick.Client]
	private void ClientTick()
	{
		DebugOverlay.Sphere( Position, 10f, Color.Red );
	}
}
