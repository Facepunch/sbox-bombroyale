using Sandbox;
using System.Linq;

namespace Facepunch.BombRoyale;

public partial class SummaryState : BaseState
{
	public override string Name => "END";
	public override bool IsPaused => true;
	public override int TimeLeft => RoundEndTime.Relative.CeilToInt();

	[Net] public string WinnerName { get; private set; }
	[Net] public int WinnerIndex { get; private set; }

	[Net] private TimeUntil RoundEndTime { get; set; }

	public override void OnEnter()
	{
		if ( Game.IsServer )
		{
			var winner = Entity.All.OfType<BombRoyalePlayer>()
				.Where( p => p.LifeState == LifeState.Alive )
				.OrderByDescending( p => p.MaxBombs + p.BombRange + p.SpeedBoosts )
				.FirstOrDefault();

			if ( winner.IsValid() )
			{
				WinnerName = winner.Client.Name;
				WinnerIndex = winner.Client.NetworkIdent;
			}

			foreach ( var bomb in Entity.All.OfType<Bomb>() )
			{
				if ( bomb.IsPlaced )
				{
					bomb.Delete();
				}
			}

			RoundEndTime = 10f;
		}
		else
		{
			if ( WinnerIndex == Game.LocalClient.NetworkIdent )
				Sound.FromScreen( "round.win" );
			else
				Sound.FromScreen( "round.end" );
		}
	}

	public override void OnLeave()
	{
		base.OnLeave();
	}

	[Event.Tick.Server]
	private void ServerTick()
	{
		if ( RoundEndTime )
		{
			var lobby = new LobbyState
			{
				RandomizeArena = true
			};

			System.Set( lobby );
		}
	}
}
