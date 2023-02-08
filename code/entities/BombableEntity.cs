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
	[Net] private bool IsHidden { get; set; }

	public override void Spawn()
	{
		EnableAllCollisions = true;
		EnableShadowCasting = true;
		Transmit = TransmitType.Always;

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		base.Spawn();
	}

	public void Hide()
	{
		var p = Pickup.CreateRandom();
		p.Position = WorldSpaceBounds.Center;

		EnableAllCollisions = false;
		IsHidden = true;
	}

	public void Show()
	{
		EnableAllCollisions = true;
		IsHidden = false;
	}

	[Event.Tick.Client]
	private void ClientTick()
	{
		EnableDrawing = !IsHidden;
	}
}
