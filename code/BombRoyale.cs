using System;
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
	public static BombRoyale Instance { get; private set; }
	
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject BombPrefab { get; set; }
	
	[Sync] public TimeUntil RoundEndTime { get; set; }

	public int RoundTimeLeft => RoundEndTime.Relative.CeilToInt();

	private List<Player> Players { get; set; } = new();

	protected override void OnAwake()
	{
		Instance = this;

		for ( var i = 0; i < 4; i++ )
		{
			Players.Add( null );
		}
		
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
			StateSystem.Set<LobbyState>();
		}
		
		base.OnStart();
	}

	private int FindFreeSlot()
	{
		for ( var i = 0; i < 4; i++ )
		{
			var player = Players[i];
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

		var spawnpoints = Scene.GetAllComponents<PlayerSpawn>().ToList();
		spawnpoints.Sort( ( a, b ) => a.Index.CompareTo( b.Index ) );

		var spawnpoint = spawnpoints[playerSlot];
		if ( !spawnpoint.IsValid() )
		{
			throw new( $"Can't find spawnpoint for player slot #{playerSlot}" );
		}

		var playerComponent = player.Components.Get<Player>();
		playerComponent.PlayerSlot = playerSlot;
		player.Transform.Position = spawnpoint.Transform.Position;
		player.Transform.Rotation = spawnpoint.Transform.Rotation;
		
		player.NetworkSpawn( connection );

		Players[playerSlot] = playerComponent;
	}
}
