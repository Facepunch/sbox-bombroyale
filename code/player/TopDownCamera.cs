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
		Camera.Position = worldBounds.Center + Vector3.Up * totalHeight * .85f + Vector3.Backward * totalHeight * .15f;
     //   Camera.ZNear = 60f;
     //   Camera.ZFar = 1200f;
        var direction = (worldBounds.Center - Camera.Position).Normal - Vector3.Backward *.002f;
		Camera.Rotation = Rotation.LookAt( direction );

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 50f );
		Camera.FirstPersonViewer = null;

		ScreenShake.Apply();

		var pawn = BombRoyalePlayer.Me;

		if ( pawn.IsValid() )
			Sound.Listener = new Transform( pawn.EyePosition, Rotation.LookAt( Vector3.Forward ) );
		else
			Sound.Listener = new Transform( Camera.Position, Camera.Rotation );
	}
}
