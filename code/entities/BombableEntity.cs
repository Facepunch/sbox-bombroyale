using Editor;
using Sandbox;

namespace Facepunch.BombsAway;

[HammerEntity]
[Title( "Bombable Entity")]
[Description( "A player blocker that can be destroyed by a bomb." )]
[SupportsSolid]
[Model]
public partial class BombableEntity : ModelEntity
{
	public override void Spawn()
	{
		EnableAllCollisions = true;
		Transmit = TransmitType.Always;

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		base.Spawn();
	}
}
