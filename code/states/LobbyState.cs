using Sandbox;
using System.Linq;

namespace Facepunch.BombRoyale;

public partial class LobbyState : BaseState
{
	public override string Name => "WAIT";
	public override int TimeLeft => RoundEndTime.Relative.CeilToInt();

	[Net] private TimeUntil RoundEndTime { get; set; }

	private bool PlayedCountdown { get; set; }
	private Sound Countdown { get; set; }

	public override void OnEnter()
	{
		if ( Game.IsServer )
		{
			foreach ( var pawn in Entity.All.OfType<BombRoyalePlayer>() )
			{
				pawn.Delete();
			}

			RoundEndTime = 10f;
		}
	}

	public override void OnLeave()
	{
		Countdown.Stop();
	}

	public override void OnPlayerJoined( BombRoyalePlayer player )
	{

	}

	[Event.Tick.Client]
	private void ClientTick()
	{
		if ( RoundEndTime <= 5f && !PlayedCountdown )
		{
			PlayedCountdown = true;
			Countdown = Sound.FromScreen( "round.countdown" );
		}
	}

	[Event.Tick.Server]
	private void ServerTick()
	{
		if ( RoundEndTime )
		{
			System.Set( new GameState() );
		}
	}
}
