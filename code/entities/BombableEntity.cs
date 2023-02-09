using Editor;
using Sandbox;
using System.Linq;

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

	public void TrySpawnPickup()
	{
		if ( Game.Random.Float() > 0.8f )
		{
			var p = Pickup.CreateRandom();
			p.Position = WorldSpaceBounds.Center;
			return;
		}
		
		if ( Game.Random.Float() >= 0.6f )
		{
			var availableBlocks = All.OfType<BombableEntity>()
				.Where( e => e.IsHidden )
				.ToList();

			var randomBlock = Game.Random.FromList( availableBlocks );

			if ( randomBlock.IsValid() )
			{
				var p = Pickup.CreateRandom();
				p.Position = randomBlock.WorldSpaceBounds.Center;
			}
		}
	}

	public void Hide()
	{
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
