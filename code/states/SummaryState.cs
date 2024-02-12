using Sandbox;
using System.Linq;

namespace Facepunch.BombRoyale;

public class SummaryState : BaseState
{
	public override string Name => "END";
	public override int TimeLeft => RoundEndTime.Relative.CeilToInt();

	[Sync] public string WinnerName { get; private set; }
	[Sync] public int WinnerIndex { get; private set; }

	[Sync] private TimeUntil RoundEndTime { get; set; }

	protected override void OnEnter()
	{
		if ( Networking.IsHost )
		{
			var winner = Scene.GetAllComponents<Player>()
				.Where( p => p.LifeState == LifeState.Alive )
				//.OrderByDescending( p => p.MaxBombs + p.BombRange + p.SpeedBoosts )
				.FirstOrDefault();

			if ( winner.IsValid() )
			{
				WinnerName = winner.Network.OwnerConnection.DisplayName;
				WinnerIndex = winner.PlayerSlot;
			}

			foreach ( var bomb in Scene.GetAllComponents<Bomb>() )
			{
				/*
				if ( bomb.IsPlaced )
				{
					bomb.Delete();
				}
				*/
			}

			RoundEndTime = 10f;
		}
		
		/*
		if ( WinnerIndex == Game.LocalClient.NetworkIdent )
			Sound.Play( "round.win" );
		else
			Sound.Play( "round.end" );
		*/
	}

	protected override void OnUpdate()
	{
		if ( Networking.IsHost )
		{
			if ( RoundEndTime )
			{
				StateSystem.Set<LobbyState>();
			}
		}
		
		base.OnUpdate();
	}
}
