namespace Facepunch.BombRoyale;

[PickupChance( 0.25f )]
public partial class SuperBomb : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/superbomb.png";
	public override Color Color => Color.Magenta;

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.HasSuperBomb = true;
	}
}
