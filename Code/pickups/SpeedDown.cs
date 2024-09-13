using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.3f )]
public class SpeedDown : Pickup
{
	public override string PickupSound => "pickup.bad";
	public override string Icon => "textures/speeddown.png";
	public override Color Color => Color.Red;

	protected override bool OnPickup( Player player )
	{
		if ( player.SpeedBoosts == 0 )
			return false;
		
		Chat.AddPlayerEvent( "pickup_bad", Network.OwnerConnection.DisplayName, player.GetTeamColor(), $"has lost some speed!" );
		
		player.SpeedBoosts = Math.Max( player.SpeedBoosts - 1, 0 );
		return true;
	}
}
