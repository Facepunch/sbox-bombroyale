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
	
	[Broadcast]
	public static void SendChat( string message )
	{
		if ( !ConsoleSystem.Caller.IsValid() ) return;

		AddChat( To.Everyone, ConsoleSystem.Caller.Name, ConsoleSystem.Caller.GetTeamColor(), message );
	}

	[Broadcast( "br.chat.system" )]
	public static void AddSystemMsgCmd( string msg )
	{
		AddSystem( msg );
	}

	[Broadcast]
	public static void AddSystem( string message )
	{
		Instance.AddMessage( message, "system" );
	}

	[Broadcast]
	public static void AddPlayerEvent( string eventName, string name, Color color, string message )
	{
		Instance.AddNamedMessage( name, color, message, eventName );
	}

	[Broadcast]
	public static void AddChat( string name, Color color, string message )
	{
		Instance.AddNamedMessage( name, color, message );
	}
}
