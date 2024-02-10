using System.Linq;
using Sandbox;
using Editor;
using Sandbox.Citizen;

namespace Facepunch.BombRoyale;

public class Player : Component
{
	[Sync] public int PlayerSlot { get; set; }
	
	private CitizenAnimationHelper Animation { get; set; }
	private CharacterController Controller { get; set; }
	private Vector2 InputDirection { get; set; }
	[Sync] private Vector3 WishVelocity { get; set; }
	
	private readonly Vector3[] Cardinals = {
		Vector3.Forward,
		Vector3.Left,
		Vector3.Right,
		Vector3.Backward
	};
	
	protected override void OnAwake()
	{
		Controller = Components.Get<CharacterController>();
		Animation = Components.Get<CitizenAnimationHelper>();
		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		UpdateAnimation();
		
		if ( IsProxy ) return;

		UpdateCamera();
		UpdateMovement();
	}

	private void UpdateAnimation()
	{
		var animator = Animation;
		animator.HoldType = CitizenAnimationHelper.HoldTypes.None;
		animator.WithVelocity( Controller.Velocity );
		animator.WithWishVelocity( WishVelocity );
		animator.IsGrounded = true;
		animator.FootShuffle = 0f;
		animator.DuckLevel = 0f;
		animator.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
	}
	
	private Vector3 SnapInputDirection( Vector3 direction )
	{
		if ( direction.Length == 0f )
			return direction;

		var output = Vector3.Zero;
		var ldp = -1f;

		foreach ( var c in Cardinals )
		{
			var dp = direction.Dot( c );
			if ( dp > ldp )
			{
				output = c;
				ldp = dp;
			}
		}

		return output;
	}
	
	private float GetWishSpeed()
	{
		/*
		if ( Player.Disease == DiseaseType.MoveFast )
			return WalkSpeed * 1.75f;
		else if ( Player.Disease == DiseaseType.MoveSlow )
			return WalkSpeed * 0.75f;

		return Scale( WalkSpeed + (25f * Player.SpeedBoosts) );
		*/
		
		return 150f;
	}
	
	private void TryDirectionalMove( Vector3 direction )
	{
		var position = Transform.Position + direction * Controller.Radius * 0.7f;
		var trace = Scene.Trace.Ray( position, position + Transform.Rotation.Forward * Controller.Radius )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( !trace.Hit )
		{
			WishVelocity += (trace.EndPosition - Transform.Position).Normal * WishVelocity.Length;
		}
	}
	
	private void UpdateMovement()
	{
		InputDirection = SnapInputDirection( Input.AnalogMove );
		
		WishVelocity = new( InputDirection.x, InputDirection.y, 0f );
		WishVelocity = WishVelocity.WithZ( 0f );
		WishVelocity *= GetWishSpeed();

		/*
		var trace = Controller.TraceDirection( WishVelocity * Time.Delta );

		if ( trace.Hit )
		{
			TryDirectionalMove( Transform.Rotation.Left );
			TryDirectionalMove( Transform.Rotation.Right );
		}
		*/
		
		Controller.Velocity = Controller.Velocity.WithZ( 0f );
		Controller.Accelerate( WishVelocity );
		Controller.ApplyFriction( 4f );

		Controller.Move();
		
		if ( InputDirection.Length > 0f )
		{
			Transform.Rotation = Rotation.Lerp( Transform.Rotation, Rotation.LookAt( InputDirection, Vector3.Up ),
				Time.Delta * 16f );
		}
	}

	private void UpdateCamera()
	{
		var boundsComponent = Scene.GetAllComponents<ArenaBounds>().FirstOrDefault();
		var worldBounds = boundsComponent.Bounds;
		var camera = Scene.Camera;
		
		var totalHeight = worldBounds.Size.Length;
		camera.Transform.Position = worldBounds.Center + Vector3.Up * totalHeight * 0.85f + Vector3.Backward * totalHeight * 0.15f;
		var direction = (worldBounds.Center - camera.Transform.Position).Normal - Vector3.Backward *.002f;
		camera.Transform.Rotation = Rotation.LookAt( direction );

		camera.FieldOfView = Screen.CreateVerticalFieldOfView( 60f );

		ScreenShake.Apply( camera );

		Sound.Listener = new( Transform.Position + Vector3.Up * 60f, Rotation.LookAt( Vector3.Forward ) );
	}
}
