using Sandbox;
using Editor;

namespace Facepunch.BombRoyale;

[Title( "Player Spawn" )]
[Category( "Bomb Royale" )]
[Icon( "accessibility_new" )]
[EditorHandle( "materials/gizmo/spawnpoint.png" )]
public sealed class PlayerSpawn : Component
{
	[Property] public int Index { get; set; }
	private Color Color { get; set; } = "#E3510D";
	
	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		var spawnpointModel = Model.Load( "models/editor/spawnpoint.vmdl" );

		Gizmo.Hitbox.Model( spawnpointModel );
		Gizmo.Draw.Color = Color.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.7f : 0.5f );
		var so = Gizmo.Draw.Model( spawnpointModel );
		
		if ( so.IsValid() )
			so.Flags.CastShadows = true;
	}
}
