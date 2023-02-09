using Sandbox;
using System.Linq;

namespace Facepunch.BombRoyale;

public partial class LobbyState : BaseState
{
	public override string Name => "WAIT";
	public override int TimeLeft => RoundEndTime.Relative.CeilToInt();

	[Net] private TimeUntil RoundEndTime { get; set; }

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

	}

	public override void OnPlayerJoined( BombRoyalePlayer player )
	{

	}

	[Event.Tick.Server]
	private  void ServerTick()
	{
		if ( RoundEndTime )
		{
			System.Set( new GameState() );
		}
	}
}
