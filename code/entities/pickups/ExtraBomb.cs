using Sandbox;
using System;

namespace Facepunch.BombRoyale;

public partial class ExtraBomb : Pickup
{
	public override string Icon => "textures/extrabomb.png";

	protected override void OnPickup( BombRoyalePlayer player )
	{
		player.MaxBombs = Math.Min( player.MaxBombs + 1, 6 );
	}
}
