namespace Facepunch.BombRoyale;

public partial class SuperBomb : Pickup
{
	public override string Icon => "textures/superbomb.png";

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.HasSuperBomb = true;
	}
}
