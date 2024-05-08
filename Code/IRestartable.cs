using Sandbox;

namespace Facepunch.BombRoyale;

public interface IRestartable
{
	public static void RestartAll()
	{
		var resettables = Game.ActiveScene.GetAllComponents<IRestartable>();
		foreach ( var resettable in resettables )
		{
			resettable.OnRestart();
		}
	}

	void OnRestart();
}
