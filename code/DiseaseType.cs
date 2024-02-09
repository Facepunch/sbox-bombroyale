using Sandbox;
using Editor;

namespace Facepunch.BombRoyale;

public enum DiseaseType
{
	None,
	MoveSlow,
	MoveFast,
	RandomBomb,
	Teleport,
	LowRange
}

public static class DiseaseTypeExtensions
{
	public static string GetName( this DiseaseType type )
	{
		return type switch
		{
			DiseaseType.MoveSlow => "⏪ Move Slow",
			DiseaseType.MoveFast => "⏩ Move Fast",
			DiseaseType.RandomBomb => "💩 Drop Random Bombs",
			DiseaseType.Teleport => "🔀 Swap Places",
			DiseaseType.LowRange => "💣 Minimum Bomb Range",
			_ => "Nothing",
		};
	}
}
