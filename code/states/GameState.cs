using Sandbox;
using System.Linq;

namespace Facepunch.BombRoyale;

public partial class GameState : BaseState
{
	public override void OnEnter()
	{
		if ( Game.IsServer )
		{
			foreach ( var client in Game.Clients )
			{
				var pawn = new BombRoyalePlayer();
				pawn.MakePawnOf( client );
				pawn.Respawn();
			}
		}
	}

	public override void OnLeave()
	{

	}
}
