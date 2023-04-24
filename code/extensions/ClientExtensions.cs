using Sandbox;

namespace Facepunch.BombRoyale;

public static class ClientExtensions
{
	private static Color[] Colors = new Color[4]
	{
		"#F6D953",
		"#DB3D76",
		"#3DBFDB",
		"#FF881B"
	};

	public static Color GetTeamColor( this IClient self )
	{
		var index = self.NetworkIdent - 1;
		return Colors[index];
	}
}
