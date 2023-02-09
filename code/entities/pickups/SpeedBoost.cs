using Sandbox;
using System;

namespace Facepunch.BombRoyale;

public partial class SpeedBoost : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/speedboost.png";
	public override Color Color => Color.Yellow;

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.SpeedBoosts = Math.Min( player.SpeedBoosts + 1, 4 );
	}
}
