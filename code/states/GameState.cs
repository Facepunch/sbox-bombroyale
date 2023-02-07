using Sandbox;
using System.Linq;

namespace Facepunch.BombsAway;

public partial class GameState : BaseState
{
	public override void OnEnter()
	{
		if ( Game.IsServer )
		{
			foreach ( var client in Game.Clients )
			{
				var pawn = new BombsAwayPlayer();
				pawn.MakePawnOf( client );
				pawn.Respawn();
			}
		}
	}

	public override void OnLeave()
	{

	}
}
