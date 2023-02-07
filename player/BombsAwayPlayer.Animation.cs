using Sandbox;

namespace Facepunch.BombsAway;

public partial class BombsAwayPlayer
{
	protected void SimulateAnimation()
	{
		Rotation = Rotation.Lerp( Rotation, ViewAngles.ToRotation(), Time.Delta * 10f );

		var animHelper = new CitizenAnimationHelper( this );
		var lookAtPosition = EyePosition + EyeRotation.Forward * 100f;

		animHelper.WithWishVelocity( Controller.WishVelocity );
		animHelper.WithVelocity( Velocity );
		animHelper.WithLookAt( lookAtPosition, 1f, 1f, 0.5f );
		animHelper.AimAngle = Rotation;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, Controller.HasTag( "ducked" ) ? 1f : 0f, Time.Delta * 10f );
		animHelper.VoiceLevel = (Game.IsClient && Client.IsValid()) ? Client.Voice.LastHeard < 0.5f ? Client.Voice.CurrentLevel : 0f : 0f;
		animHelper.IsGrounded = GroundEntity != null;
		animHelper.IsSitting = Controller.HasTag( "sitting" );
		animHelper.IsNoclipping = Controller.HasTag( "noclip" );
		animHelper.IsClimbing = Controller.HasTag( "climbing" );
		animHelper.IsSwimming = false;
		animHelper.Handedness = CitizenAnimationHelper.Hand.Both;
		animHelper.IsWeaponLowered = false;

		if ( Controller.HasEvent( "jump" ) ) animHelper.TriggerJump();

		animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		animHelper.AimBodyWeight = 0.5f;
	}
}
