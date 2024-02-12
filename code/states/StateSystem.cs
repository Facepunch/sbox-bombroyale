using Sandbox;
using Sandbox.Diagnostics;

namespace Facepunch.BombRoyale;

public static class StateSystem
{
	public static BaseState Active { get; internal set; }

	public static T Set<T>() where T : BaseState, new()
	{
		Assert.True( Networking.IsHost );

		if ( Active.IsValid() )
		{
			Active.GameObject.DestroyImmediate();
		}

		var go = new GameObject();
		var state = go.Components.Create<T>();
		go.Name = "State";
		go.NetworkSpawn();

		return state;
	}
}
