using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.2f )]
public partial class ExtraLife : Pickup
{
	public override string PickupSound => "pickup.extralife";
	public override string Icon => "textures/extralife.png";
	public override Color Color => Color.Green;

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.LivesLeft = Math.Min( player.LivesLeft + 1, 2 );
	}
}
