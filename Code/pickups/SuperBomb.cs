namespace Facepunch.BombRoyale;

[PickupChance( 0.25f )]
public class SuperBomb : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/superbomb.png";
	public override Color Color => Color.Yellow;

	protected override bool OnPickup( Player player )
	{
		if ( player.HasSuperBomb )
			return false;
		
		Chat.AddPlayerEvent( "pickup", Network.OwnerConnection.DisplayName, player.GetTeamColor(), $"has picked up a Super Bomb!" );
		
		player.HasSuperBomb = true;
		return true;
	}
}
