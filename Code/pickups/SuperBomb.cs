namespace Facepunch.BombRoyale;

[PickupChance( 0.25f )]
public class SuperBomb : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/superbomb.png";
	public override Color Color => Color.Yellow;

	protected override void OnPickup( Player player )
	{
		player.HasSuperBomb = true;
	}
}
