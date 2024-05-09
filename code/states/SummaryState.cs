using Sandbox;
using System.Linq;

namespace Facepunch.BombRoyale;

public class SummaryState : BaseState
{
	public override string Name => "END";
	public override int TimeLeft => RoundEndTime.Relative.CeilToInt();
	public override bool IsPaused => true;

	[Sync] public string WinnerName { get; private set; }
	[Sync] public int WinnerIndex { get; private set; }

	[Sync] private TimeUntil RoundEndTime { get; set; }

	protected override void OnEnter()
	{
		if ( Networking.IsHost )
		{
			var winner = BombRoyale.Players
				.Where( p => p.LifeState == LifeState.Alive )
				.MaxBy( p => p.MaxBombs + p.BombRange + p.SpeedBoosts );

			if ( winner.IsValid() )
			{
				WinnerName = winner.Network.OwnerConnection.DisplayName;
				WinnerIndex = winner.PlayerSlot;
			}

			foreach ( var bomb in Scene.GetAllComponents<Bomb>() )
			{
				if ( bomb.IsPlaced )
				{
					bomb.Destroy();
				}
			}

			RoundEndTime = 10f;
		}

		var localPlayer = Player.Me;
		
		if ( localPlayer.IsValid() && WinnerIndex == localPlayer.PlayerSlot )
			Sound.Play( "round.win" );
		else
			Sound.Play( "round.end" );
	}

	protected override void OnUpdate()
	{
		if ( Networking.IsHost && RoundEndTime )
		{
			StateSystem.Set<LobbyState>();
		}
		
		base.OnUpdate();
	}
}
