using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.3f )]
public partial class RangeDown : Pickup
{
	public override string PickupSound => "pickup.bad";
	public override string Icon => "textures/rangedown.png";
	public override Color Color => Color.Cyan;

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.BombRange = Math.Max( player.BombRange - 1, 2 );
	}
}
