﻿using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.2f )]
public class SpeedBoost : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/speedboost.png";
	public override Color Color => Color.Orange;

	protected override void OnPickup( Player player )
	{
		player.SpeedBoosts = Math.Min( player.SpeedBoosts + 1, 4 );
	}
}
