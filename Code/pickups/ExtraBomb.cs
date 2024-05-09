using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.7f )]
public class ExtraBomb : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/extrabomb.png";
	public override Color Color => Color.Green;

	protected override void OnPickup( Player player )
	{
		player.MaxBombs = Math.Min( player.MaxBombs + 1, 6 );
	}
}
