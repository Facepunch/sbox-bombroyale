using Sandbox;

namespace Facepunch.BombRoyale;

public partial class TopDownCamera
{
	public float Height { get; set; } = 450f;
	public float MoveSpeed { get; set; } = 20f;

	public void Update()
	{
		var arena = BombRoyaleGame.Arena;

		BBox worldBounds;

		if ( arena.IsValid() )
			worldBounds = arena.WorldSpaceBounds;
		else
			worldBounds = Game.PhysicsWorld.Body.GetBounds();

		var totalHeight = worldBounds.Size.Length;
		Camera.Position = worldBounds.Center + Vector3.Up * totalHeight * 1.5f + Vector3.Backward * totalHeight * 0.5f;

		var direction = (worldBounds.Center - Camera.Position).Normal;
		Camera.Rotation = Rotation.LookAt( direction );

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 30f );
		Camera.FirstPersonViewer = null;

		ScreenShake.Apply();

		var pawn = BombRoyalePlayer.Me;

		if ( pawn.IsValid() )
			Sound.Listener = new Transform( pawn.EyePosition, Rotation.LookAt( Vector3.Forward ) );
		else
			Sound.Listener = new Transform( Camera.Position, Camera.Rotation );
	}
}
