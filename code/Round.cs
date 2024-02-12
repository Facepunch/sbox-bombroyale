using Sandbox;
using Editor;

namespace Facepunch.BombRoyale;

public enum Round
{
	Lobby,
	Playing,
	GameOver
}

public static class RoundExtension
{
	public static string GetName( this Round round )
	{
		return round switch
		{
			Round.Lobby => "Lobby",
			Round.Playing => "Playing",
			Round.GameOver => "Game Over",
			_ => string.Empty
		};
	}
}
