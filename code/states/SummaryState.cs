using Sandbox;

namespace Facepunch.BombRoyale;

public partial class SummaryState : BaseState
{
	public override string Name => "END";
	public override int TimeLeft => RoundEndTime.Relative.CeilToInt();

	[Net] private TimeUntil RoundEndTime { get; set; }

	public override void OnEnter()
	{
		if ( Game.IsServer )
		{
			RoundEndTime = 10f;
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
