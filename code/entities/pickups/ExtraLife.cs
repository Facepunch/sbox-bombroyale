using Sandbox;
using System;

namespace Facepunch.BombRoyale;

public partial class ExtraLife : Pickup
{
	public override string Icon => "textures/extralife.png";

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.LivesLeft = Math.Min( player.LivesLeft + 1, 2 );
	}
}
