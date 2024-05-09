using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.7f )]
public class RangeUp : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/rangeup.png";
	public override Color Color => Color.Cyan;

	protected override void OnPickup( Player player )
	{
		player.BombRange = Math.Min( player.BombRange + 1, 8 );
	}
}
