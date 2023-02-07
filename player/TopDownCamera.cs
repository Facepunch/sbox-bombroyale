﻿using Sandbox;

namespace Facepunch.BombsAway;

public partial class TopDownCamera
{
	public float Height { get; set; } = 450f;
	public float MoveSpeed { get; set; } = 20f;

	public void Update()
	{
		var pawn = BombsAwayPlayer.Me;

		if ( pawn.IsValid() )
		{
			Sound.Listener = new Transform( pawn.EyePosition, Camera.Rotation );

			var worldBounds = Game.PhysicsWorld.Body.GetBounds();
			var totalHeight = worldBounds.Size.Length;

			Camera.Position = worldBounds.Center + Vector3.Up * totalHeight * 1.5f + Vector3.Backward * totalHeight * 0.5f;

			var direction = (worldBounds.Center - Camera.Position).Normal;
			Camera.Rotation = Rotation.LookAt( direction );
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 30f );

			/*
			Camera.Position = worldBounds.Center + Vector3.Up * 1000f;
			Camera.Main.Ortho = true;
			Camera.Main.OrthoWidth = worldBounds.Size.x;
			Camera.Main.OrthoHeight = worldBounds.Size.y;
			*/

			Camera.FirstPersonViewer = null;
		}
	}
}
