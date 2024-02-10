using Sandbox;
using Editor;

namespace Facepunch.BombRoyale;

[Title( "Arena Bounds" )]
[Category( "Bomb Royale" )]
public sealed class ArenaBounds : Component
{
	[Property] public Vector3 Size { get; set; }

	public BBox Bounds => new BBox( Transform.Position - Size * 0.5f, Transform.Position + Size * 0.5f );
	
	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		var center = Transform.Position;
		var mins = center - Size * 0.5f;
		var maxs = center + Size * 0.5f;

		Gizmo.Draw.Color = Color.Cyan;
		Gizmo.Draw.LineBBox( new( mins, maxs ) );
	}
}
