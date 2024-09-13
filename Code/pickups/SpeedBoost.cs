using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.2f )]
public class SpeedBoost : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/speedboost.png";
	public override Color Color => Color.Orange;

	protected override bool OnPickup( Player player )
	{
		if ( player.SpeedBoosts == 4 )
			return false;
		
		Chat.AddPlayerEvent( "pickup", Network.OwnerConnection.DisplayName, player.GetTeamColor(), $"has gained some speed!" );
		
		var previousSpeed = player.SpeedBoosts;
		player.SpeedBoosts = Math.Min( player.SpeedBoosts + 1, 4 );

		if ( previousSpeed < 4 && player.SpeedBoosts == 4 )
			player.UnlockAchievement( "go_fast" );
		
		return true;
	}
}
