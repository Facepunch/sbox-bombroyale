using Sandbox;
using System;

namespace Facepunch.BombRoyale;

[PickupChance( 0.2f )]
public class Disease : Pickup
{
	public override string PickupSound => "disease.pickup";
	public override string Icon => "textures/disease.png";
	public override Color Color => Color.Magenta;

	protected override void OnPickup( Player player )
	{
		var diseases = Enum.GetValues<DiseaseType>();
		var index = Game.Random.Int( 1, diseases.Length - 1 );

		player.GiveDisease( diseases[index] );
	}
}
