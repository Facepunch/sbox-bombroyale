using Sandbox;
using Sandbox.Effects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Facepunch.BombRoyale;

public partial class BombRoyaleGame : GameManager
{
	public static BombRoyaleGame Entity => Current as BombRoyaleGame;
	public static StateSystem StateSystem => Entity?.InernalStateSystem;
	public static Arena Arena => Entity?.InternalArena;

	[Net] private StateSystem InernalStateSystem { get; set; }
	[Net] private Arena InternalArena { get; set; }

	private TopDownCamera TopDownCamera { get; set; }
	private ScreenEffects PostProcessing { get; set; }

	public BombRoyaleGame() : base()
	{

	}

	public static void ChooseRandomArena()
	{
		if ( !Current.IsValid() ) return;

		var arenas = All.OfType<Arena>().ToList();
		var random = Game.Random.FromList( arenas );
		if ( random == null ) return;

		Entity.InternalArena = random;
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
		PostProcessing = new();

		Camera.Main.AddHook( PostProcessing );

		base.ClientSpawn();
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		List<Entity> spawnpoints;

		if ( InternalArena.IsValid() )
		{
			spawnpoints = All.OfType<PlayerSpawnpoint>()
				.Where( e => e.ArenaId == InternalArena.ArenaId )
				.Select( e => e as Entity )
				.ToList();
		}
		else
		{
			spawnpoints = All.OfType<SpawnPoint>()
				.Select( e => e as Entity )
				.ToList();
		}

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
		ChooseRandomArena();

		base.PostLevelLoaded();
	}

	[Event.Tick.Server]
	private void ServerTick()
	{
		
	}

	[Event.Client.Frame]
	private void OnFrame()
	{
		var sum = ScreenShake.List.OfType<ScreenShake.Random>().Sum( s => (1f - s.Delta) );
		PostProcessing.Pixelation = 0.05f * sum;
		TopDownCamera?.Update();
	}
}
