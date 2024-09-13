using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.3f )]
public class RangeDown : Pickup
{
	public override string PickupSound => "pickup.bad";
	public override string Icon => "textures/rangedown.png";
	public override Color Color => Color.Red;

	protected override bool OnPickup( Player player )
	{
		if ( player.BombRange == 2 )
			return false;
		
		Chat.AddPlayerEvent( "pickup_bad", Network.OwnerConnection.DisplayName, player.GetTeamColor(), $"has lost some bomb range!" );
		
		player.BombRange = Math.Max( player.BombRange - 1, 2 );
		return true;
	}
}
