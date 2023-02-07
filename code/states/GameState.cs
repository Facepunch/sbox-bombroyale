using Sandbox;
using System.Linq;

namespace Facepunch.BombsAway;

public partial class GameState : BaseState
{
	public override void OnEnter()
	{
		if ( Game.IsServer )
		{
			foreach ( var player in Entity.All.OfType<BombsAwayPlayer>() )
			{
				player.Respawn();
			}
		}
	}

	public override void OnLeave()
	{

	}
}
