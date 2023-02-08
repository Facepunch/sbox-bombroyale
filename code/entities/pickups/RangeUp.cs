using Sandbox;
using System;

namespace Facepunch.BombRoyale;

public partial class RangeUp : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/rangeup.png";

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.BombRange = Math.Min( player.BombRange + 1, 8 );
	}
}
