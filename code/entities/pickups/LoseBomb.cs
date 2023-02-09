using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.35f )]
public partial class LoseBomb : Pickup
{
	public override string PickupSound => "pickup.bad";
	public override string Icon => "textures/losebomb.png";
	public override Color Color => Color.Red;

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.MaxBombs = Math.Max( player.MaxBombs - 1, 1 );
	}
}
