using Sandbox;

namespace Facepunch.BombRoyale;

[Title( "Solid Block" )]
[Category( "Bomb Royale" )]
public sealed class SolidBlock : Component
{
	protected override void OnAwake()
	{
		Tags.Add( "solid" );
		base.OnAwake();
	}
}
