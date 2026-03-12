using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.35f )]
public class LoseBomb : Pickup
{
	public override string PickupSound => "pickup.bad";
	public override string Icon => "textures/losebomb.png";
	public override Color Color => Color.Red;

	protected override bool OnPickup( Player player )
	{
		if ( player.MaxBombs > 1 )
		{
			player.MaxBombs = Math.Max( player.MaxBombs - 1, 1 );
			Chat.AddPlayerEvent( "pickup_bad", Network.Owner.DisplayName, player.GetTeamColor(), $"has lost an extra bomb!" );
		}

		return true;
	}
}
