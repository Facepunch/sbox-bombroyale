using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.7f )]
public class RangeUp : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/rangeup.png";
	public override Color Color => Color.Cyan;

	protected override bool OnPickup( Player player )
	{
		if ( player.BombRange == 8 )
			return false;
		
		Chat.AddPlayerEvent( "pickup", Network.OwnerConnection.DisplayName, player.GetTeamColor(), $"has gained some bomb range!" );
		
		player.BombRange = Math.Min( player.BombRange + 1, 8 );
		return true;
	}
}
