using Sandbox;

namespace Facepunch.BombRoyale;

public partial class BombRoyalePlayer
{
	protected void SimulateAnimation()
	{
		Rotation = Rotation.Lerp( Rotation, ViewAngles.ToRotation(), Time.Delta * 20f );

		var animHelper = new CitizenAnimationHelper( this );
		var lookAtPosition = EyePosition + EyeRotation.Forward * 100f;

		animHelper.WithWishVelocity( Controller.WishVelocity );
		animHelper.WithVelocity( Velocity );
		animHelper.WithLookAt( lookAtPosition, 1f, 1f, 0.5f );
		animHelper.AimAngle = Rotation;
		animHelper.DuckLevel = 0f;
		animHelper.VoiceLevel = (Game.IsClient && Client.IsValid()) ? Client.Voice.LastHeard < 0.5f ? Client.Voice.CurrentLevel : 0f : 0f;
		animHelper.IsGrounded = true;
		animHelper.IsSitting = false;
		animHelper.IsNoclipping = false;
		animHelper.IsClimbing = false;
		animHelper.IsSwimming = false;
		animHelper.Handedness = CitizenAnimationHelper.Hand.Both;
		animHelper.IsWeaponLowered = false;
		animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;

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
