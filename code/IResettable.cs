using Sandbox;

namespace Facepunch.BombRoyale;

public interface IResettable
{
	public static void ResetAll()
	{
		var resettables = GameManager.ActiveScene.GetAllComponents<IResettable>();
		foreach ( var resettable in resettables )
		{
			resettable.Reset();
		}
	}

	void Reset();
}
