using Sandbox;
using Sandbox.Utility;

namespace Facepunch.BombRoyale;

public partial class ScreenShake
{
	public class Random : ScreenShake
	{
		private float Length { get; set; } = 5f;
		private float Size { get; set; } = 1f;
		private TimeSince LifeTime { get; set; } = 0f;

		public Random( float length = 1.5f, float size = 1f )
		{
			Length = length;
			Size = size;
		}

		public override bool Update()
		{
			var delta = ((float)LifeTime).LerpInverse( 0, Length, true );
			delta = Easing.EaseOut( delta );

			var random = Vector3.Random;
			random.z = 0;
			random = random.Normal;

			Camera.Position += (Camera.Rotation.Right * random.x + Camera.Rotation.Up * random.y) * (1f - delta) * Size;

			return LifeTime < Length;
		}
	}
}
