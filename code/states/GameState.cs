using Sandbox;
using System.Linq;

namespace Facepunch.BombRoyale;

public class GameState : BaseState
{
	public override string Name => "FIGHT";
	public override int TimeLeft => RoundEndTime.Relative.CeilToInt();

	[Sync] private TimeUntil RoundEndTime { get; set; }

	private SoundHandle Music { get; set; }

	protected override void OnEnter()
	{
		if ( Networking.IsHost )
		{
			IResettable.ResetAll();
			
			foreach ( var player in BombRoyale.Players )
			{
				player.Respawn();
			}
			
			RoundEndTime = 180f;
		}
		
		Sound.Play( "round.start" );
		Music = Sound.Play( "battle.music" );
	}

	protected override void OnUpdate()
	{
		if ( Networking.IsHost )
		{
			var alivePlayers = BombRoyale.Players
				.Count( p => p.LifeState == LifeState.Alive );

			if ( RoundEndTime || ( Connection.All.Count > 1 && alivePlayers <= 1 ) )
			{
				StateSystem.Set<SummaryState>();
			}
		}
		
		base.OnUpdate();
	}

	protected override void OnLeave()
	{
		Music?.Stop();
		Music = null;
	}
}
