using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Network;

namespace Facepunch.BombRoyale;

[Title( "Gamemode" )]
[Category( "Bomb Royale" )]
public class BombRoyale : Component, Component.INetworkListener
{
	public static IEnumerable<Player> Players => InternalPlayers.Where( p => p.IsValid() );
	private static List<Player> InternalPlayers { get; set; } = new( 4 ) { null, null, null, null };
	
	public static BombRoyale Instance { get; private set; }
	
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject BombPrefab { get; set; }
	
	public static bool IsPaused => StateSystem.Active?.IsPaused ?? false;

	public static Player GetPlayer( int slot ) => InternalPlayers[slot];

	public static void AddPlayer( int slot, Player player )
	{
		player.PlayerSlot = slot;
		InternalPlayers[slot] = player;
	}

	protected override void OnAwake()
	{
		Instance = this;
		base.OnAwake();
	}
	
	protected override void OnStart()
	{
		if ( !GameNetworkSystem.IsActive )
		{
			GameNetworkSystem.CreateLobby();
		}

		if ( Networking.IsHost )
		{
			var state = StateSystem.Set<LobbyState>();
			state.RoundEndTime = 5f;
		}

		ScreenShake.ClearAll();
		
		base.OnStart();
	}

	private int FindFreeSlot()
	{
		for ( var i = 0; i < 4; i++ )
		{
			var player = InternalPlayers[i];
			if ( player.IsValid() ) continue;
			return i;
		}

		return -1;
	}

	void INetworkListener.OnActive( Connection connection )
	{
		var player = PlayerPrefab.Clone();
		var playerSlot = FindFreeSlot();

		if ( playerSlot < 0 )
		{
			throw new( "Player joined but there's no free slots!" );
		}

		var playerComponent = player.Components.Get<Player>();
		AddPlayer( playerSlot, playerComponent );
		player.NetworkSpawn( connection );
	}
}
