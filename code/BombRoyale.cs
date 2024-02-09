using Sandbox;
using Sandbox.Network;

namespace Facepunch.BombRoyale;

public class BombRoyale : Component, Component.INetworkListener
{
	public static BombRoyale Instance { get; private set; }
	
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject BombPrefab { get; set; }

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
		
		base.OnStart();
	}

	void INetworkListener.OnActive( Connection connection )
	{
		var player = PlayerPrefab.Clone();
		player.NetworkSpawn( connection );
	}
}
