using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.7f )]
public class ExtraBomb : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/extrabomb.png";
	public override Color Color => Color.Green;

	protected override bool OnPickup( Player player )
	{
		if ( player.MaxBombs == 6 )
			return false;
		
		Chat.AddPlayerEvent( "pickup", Network.OwnerConnection.DisplayName, player.GetTeamColor(), $"has picked up an extra bomb!" );
		player.MaxBombs = Math.Min( player.MaxBombs + 1, 6 );
		return true;
	}
}
