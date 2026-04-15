using System.Collections.Generic;
using Sandbox;

namespace Facepunch.BombRoyale;

[GameResource( "Arena Theme", "theme", "A visual theme for arena generation (block models and floor material).", Icon = "park" )]
public class ArenaTheme : GameResource
{
	public List<Model> BreakableBlockModels { get; set; } = new();
	public List<Model> SolidBlockModels { get; set; } = new();
	public Material FloorMaterial { get; set; }
	public Color BackgroundColor { get; set; } = Color.Black;
	public GameObject AmbiencePrefab { get; set; }
}
