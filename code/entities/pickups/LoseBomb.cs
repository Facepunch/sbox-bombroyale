using Sandbox;
using System;

namespace Facepunch.BombRoyale;

public partial class LoseBomb : Pickup
{
	public override string Icon => "textures/losebomb.png";

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.MaxBombs = Math.Max( player.MaxBombs - 1, 1 );
	}
}
