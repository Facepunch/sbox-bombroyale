using Sandbox;
using System.Linq;

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
			Sound.FromScreen( To.Everyone, "round.start" );

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
			Sound.FromScreen( "round.end" );
			Music.Stop();
		}
	}

	[Event.Tick.Server]
	private void ServerTick()
	{
		var alivePlayers = Entity.All.OfType<BombRoyalePlayer>()
			.Where( p => p.LifeState == LifeState.Alive )
			.Count();

		if ( RoundEndTime || ( Game.Clients.Count > 1 && alivePlayers < 1 ) )
		{
			System.Set( new SummaryState() );
		}
	}
}
