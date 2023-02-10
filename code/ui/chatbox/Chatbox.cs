using Sandbox;
using Sandbox.UI;

namespace Facepunch.BombRoyale.UI;

public partial class Chatbox : Panel
{
	private static Chatbox Instance;

	public Chatbox()
	{
		Instance = this;
	}
	
	[ConCmd.Server]
	public static void SendChat( string message )
	{
		if ( !ConsoleSystem.Caller.IsValid() ) return;

		AddChat( To.Everyone, ConsoleSystem.Caller.Name, ConsoleSystem.Caller.GetTeamColor(), message );
	}

	[ConCmd.Server( "br.chat.system" )]
	public static void AddSystemMsgCmd( string msg )
	{
		AddSystem( msg );
	}

	[ClientRpc]
	public static void AddSystem( string message )
	{
		Instance.AddMessage( message, "system" );
	}

	[ConCmd.Client( "br.say", CanBeCalledFromServer = true )]
	public static void AddChat( string name, Color color, string message )
	{
		Instance.AddNamedMessage( name, color, message );
	}
}
