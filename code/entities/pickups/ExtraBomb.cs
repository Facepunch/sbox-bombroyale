using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.7f )]
public partial class ExtraBomb : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/extrabomb.png";
	public override Color Color => Color.Cyan;

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.MaxBombs = Math.Min( player.MaxBombs + 1, 6 );
	}
}
