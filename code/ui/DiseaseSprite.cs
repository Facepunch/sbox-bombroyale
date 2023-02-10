using Sandbox;
using Sandbox.UI;
using System;

namespace Facepunch.BombRoyale;

[StyleSheet( "ui/DiseaseSprite.scss" )]
public class DiseaseSprite : WorldPanel
{
	private BombRoyalePlayer Player { get; set; }

	public DiseaseSprite( BombRoyalePlayer player ) : base()
	{
		Player = player;
		AddChild<Panel>( "icon" );
	}

	public override void Tick()
	{
		if ( IsDeleting ) return;

		if ( Player.IsValid() && Player.LifeState == LifeState.Alive )
		{
			var transform = Transform;
			var position = Player.EyePosition + Vector3.Up * 80f;
			var direction = (Camera.Position - position).Normal;
			var targetRotation = Rotation.LookAt( direction );

			transform.Position = position;
			transform.Rotation = targetRotation;
			transform.Scale = 0.9f + (0.1f + (MathF.Cos( Time.Now * 4f ) * 0.1f));

			Transform = transform;
		}
		else
		{
			Delete();
		}

		base.Tick();
	}
}
