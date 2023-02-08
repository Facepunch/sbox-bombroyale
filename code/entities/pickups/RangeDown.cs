using Sandbox;
using System;

namespace Facepunch.BombRoyale;

public partial class RangeDown : Pickup
{
	public override string PickupSound => "pickup.bad";
	public override string Icon => "textures/rangedown.png";

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.BombRange = Math.Max( player.BombRange - 1, 2 );
	}
}
