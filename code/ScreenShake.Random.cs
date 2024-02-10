using Sandbox;
using Sandbox.Utility;

namespace Facepunch.BombRoyale;

public partial class ScreenShake
{
	public class Random : ScreenShake
	{
		public float Progress => Easing.EaseOut( ((float)LifeTime).LerpInverse( 0, Length, true ) );

		private float Length { get; set; }
		private float Size { get; set; }
		private TimeSince LifeTime { get; set; } = 0f;

		public Random( float length = 1.5f, float size = 1f )
		{
			Length = length;
			Size = size;
		}

		protected override bool Update( CameraComponent camera )
		{
			var random = Vector3.Random;
			random.z = 0f;
			random = random.Normal;

			camera.Transform.Position += (camera.Transform.Rotation.Right * random.x + camera.Transform.Rotation.Up * random.y) * (1f - Progress) * Size;

			return LifeTime < Length;
		}
	}
}
