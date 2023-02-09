﻿using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.9f )]
public partial class RangeUp : Pickup
{
	public override string PickupSound => "pickup.good";
	public override string Icon => "textures/rangeup.png";
	public override Color Color => Color.Orange;

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.BombRange = Math.Min( player.BombRange + 1, 8 );
	}
}
