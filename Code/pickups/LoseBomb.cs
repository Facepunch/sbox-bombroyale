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
		if ( player.MaxBombs == 1 )
			return false;
		
		Chat.AddPlayerEvent( "pickup", Network.OwnerConnection.DisplayName, player.GetTeamColor(), $"has lost an extra bomb!" );
		player.MaxBombs = Math.Max( player.MaxBombs - 1, 1 );
		return true;
	}
}
