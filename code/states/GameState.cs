using Sandbox;

namespace Facepunch.BombRoyale;

public partial class GameState : BaseState
{
	public override string Name => "FIGHT";
	public override int TimeLeft => RoundEndTime.Relative.CeilToInt();

	[Net] private TimeUntil RoundEndTime { get; set; }

	private Sound Music { get; set; }

	public override void OnEnter()
	{
		if ( Game.IsServer )
		{
			IResettable.ResetAll();

			foreach ( var client in Game.Clients )
			{
				var pawn = new BombRoyalePlayer();
				pawn.MakePawnOf( client );
				pawn.Respawn();
			}

			RoundEndTime = 120f;
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

	[Event.Tick.Server]
	private void ServerTick()
	{
		if ( RoundEndTime )
		{
			System.Set( new SummaryState() );
		}
	}
}
