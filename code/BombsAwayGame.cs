using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Facepunch.BombsAway;

public partial class BombsAwayGame : GameManager
{
	public static BombsAwayGame Entity => Current as BombsAwayGame;
	public static StateSystem StateSystem => Entity?.InernalStateSystem;

	[Net] private StateSystem InernalStateSystem { get; set; }

	private TopDownCamera TopDownCamera { get; set; }

	public BombsAwayGame() : base()
	{

	}

	public override void Spawn()
	{
		InernalStateSystem = new();
		InernalStateSystem.Set( new LobbyState() );

		base.Spawn();
	}

	public override void ClientSpawn()
	{
		Game.RootPanel?.Delete( true );
		Game.RootPanel = new UI.Hud();

		TopDownCamera = new();

		base.ClientSpawn();
	}

	public override void ClientJoined( IClient client )
	{
		var pawn = new BombsAwayPlayer();
		pawn.MakePawnOf( client );
		pawn.Respawn();

		base.ClientJoined( client );
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		var spawnpoints = All.OfType<SpawnPoint>().ToList();
		var index = pawn.Client.NetworkIdent - 1;

		if ( index >= spawnpoints.Count )
		{
			base.MoveToSpawnpoint( pawn );
			return;
		}

		var spawnpoint = spawnpoints[index];
		pawn.Transform = spawnpoint.Transform;
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( client, reason );
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();
	}

	[Event.Tick.Server]
	private void ServerTick()
	{
		
	}

	[Event.Client.Frame]
	private void OnFrame()
	{
		TopDownCamera?.Update();
	}
}
