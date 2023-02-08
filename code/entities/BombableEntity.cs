using Editor;
using Sandbox;

namespace Facepunch.BombRoyale;

[HammerEntity]
[Title( "Bombable Entity")]
[Description( "A player blocker that can be destroyed by a bomb." )]
[SupportsSolid]
[Model]
public partial class BombableEntity : ModelEntity
{
	private bool IsHidden { get; set; }

	public override void Spawn()
	{
		EnableAllCollisions = true;
		Transmit = TransmitType.Always;

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		base.Spawn();
	}

	public void Hide()
	{
		var p = new Pickup();
		p.Position = WorldSpaceBounds.Center;

		EnableAllCollisions = false;
		EnableDrawing = false;
		IsHidden = true;
	}

	public void Show()
	{
		EnableAllCollisions = true;
		EnableDrawing = true;
		IsHidden = false;
	}
}
