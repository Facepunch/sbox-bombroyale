using Sandbox;
using Sandbox.UI;
using System;

namespace Facepunch.BombRoyale;

[StyleSheet( "ui/PickupSprite.scss" )]
public class PickupSprite : WorldPanel
{
	private Pickup Pickup { get; set; }

	public PickupSprite( Pickup pickup ) : base()
	{
		Pickup = pickup;

		var icon = AddChild<Panel>( "icon" );

		if ( !string.IsNullOrEmpty( pickup.Icon ) )
		{
			icon.Style.SetBackgroundImage( pickup.Icon );
		}
	}

	public override void Tick()
	{
		if ( Pickup.IsValid() )
		{
			var transform = Transform;
			var direction = (Camera.Position - Pickup.Position).Normal;
			var targetRotation = Rotation.LookAt( direction );

			transform.Position = Pickup.Position + Vector3.Up * (16f + (MathF.Sin( Time.Now * 4f ) * 4f));
			transform.Rotation = targetRotation;

			Transform = transform;
		}

		base.Tick();
	}
}
