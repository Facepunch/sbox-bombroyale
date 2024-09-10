using Facepunch.BombRoyale;

namespace Sandbox;

public class DiseaseSpreader : Component, Component.ITriggerListener
{
	[Property] public Player Player { get; set; }
	
	public TimeUntil IsActive { get; set; }
	
	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( !Networking.IsHost ) return;
		if ( !IsActive ) return;
		
		var otherPlayer = other.Components.GetInAncestorsOrSelf<Player>();
		if ( !otherPlayer.IsValid() ) return;

		if ( otherPlayer.Disease == DiseaseType.None )
			return;

		if ( Player.Disease != DiseaseType.None )
			return;

		otherPlayer.UnlockAchievement( "catch_disease" );
		Player.GiveDisease( otherPlayer.Disease );
	}
}
