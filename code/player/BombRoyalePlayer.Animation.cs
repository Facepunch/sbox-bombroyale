using Sandbox;

namespace Facepunch.BombRoyale;

public partial class BombRoyalePlayer
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
		animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;

		if ( Controller.HasEvent( "jump" ) ) animHelper.TriggerJump();

		if ( HoldingBomb.IsValid() )
		{
			var lhp = Vector3.Left * 11f + Vector3.Up * 66f + Vector3.Forward * 8f;
			var rhp = Vector3.Right * 11f + Vector3.Up * 66f + Vector3.Forward * 8f;

			SetAnimParameter( "b_vr", true );
			SetAnimParameter( "left_hand_ik.position", lhp );
			SetAnimParameter( "left_hand_ik.rotation", Rotation.LookAt( Vector3.Up, Vector3.Backward )
				.RotateAroundAxis( Vector3.Forward, 90f )
				.RotateAroundAxis( Vector3.Left, 30f )
			);
			SetAnimParameter( "right_hand_ik.position", rhp );
			SetAnimParameter( "right_hand_ik.rotation", Rotation.LookAt( Vector3.Up, Vector3.Backward )
				.RotateAroundAxis( Vector3.Forward, 90f )
				.RotateAroundAxis( Vector3.Left, -30f )
			);
		}
		else
		{
			SetAnimParameter( "b_vr", false );
		}

		animHelper.AimBodyWeight = 0.5f;
	}
}
