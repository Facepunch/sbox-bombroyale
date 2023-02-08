using Sandbox;

namespace Facepunch.BombRoyale;

public partial class GameState : BaseState
{
	private Sound Music { get; set; }

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
		else
		{
			Music = Sound.FromScreen( "battle.music" );
		}
	}

	public override void OnLeave()
	{
		if ( Game.IsClient )
		{
			Music.Stop();
		}
	}
}
