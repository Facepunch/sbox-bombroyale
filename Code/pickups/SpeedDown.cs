using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.3f )]
public class SpeedDown : Pickup
{
	public override string PickupSound => "pickup.bad";
	public override string Icon => "textures/speeddown.png";
	public override Color Color => Color.Red;

	protected override void OnPickup( Player player )
	{
		player.SpeedBoosts = Math.Max( player.SpeedBoosts - 1, 0 );
	}
}
