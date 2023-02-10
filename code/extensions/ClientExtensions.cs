using Sandbox;

namespace Facepunch.BombRoyale;

public static class ClientExtensions
{
	private static Color[] Colors = new Color[4]
	{
		(Color)"#F6D953",
		(Color)"#DB3D76",
		(Color)"#3DBFDB",
		(Color)"#FF881B"
	};

	public static Color GetTeamColor( this IClient self )
	{
		var index = self.NetworkIdent - 1;
		return Colors[index];
	}
}
