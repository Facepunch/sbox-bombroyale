﻿using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.3f )]
public class RangeDown : Pickup
{
	public override string PickupSound => "pickup.bad";
	public override string Icon => "textures/rangedown.png";
	public override Color Color => Color.Red;

	protected override void OnPickup( Player player )
	{
		player.BombRange = Math.Max( player.BombRange - 1, 2 );
	}
}
