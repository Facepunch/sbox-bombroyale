using Sandbox;
using System;

namespace Facepunch.BombRoyale;

public partial class SpeedBoost : Pickup
{
	public override string Icon => "textures/speedboost.png";

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.SpeedBoosts = Math.Min( player.SpeedBoosts + 1, 4 );
	}
}
