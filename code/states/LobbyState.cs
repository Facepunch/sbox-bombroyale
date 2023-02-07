using Sandbox;
using System.Linq;

namespace Facepunch.BombsAway;

public partial class LobbyState : BaseState
{
	[Net] public RealTimeUntil StateEndTime { get; set; }
	public float StateDuration => 10f;

	public override void OnEnter()
	{
		if ( Game.IsServer )
		{
			foreach ( var pawn in Entity.All.OfType<BombsAwayPlayer>() )
			{
				pawn.Delete();
			}

			StateEndTime = StateDuration;
		}
	}

	public override void OnLeave()
	{

	}

	public override void OnPlayerJoined( BombsAwayPlayer player )
	{

	}

	[Event.Tick.Server]
	protected virtual void ServerTick()
	{
		if ( StateEndTime )
		{
			System.Set( new GameState() );
		}
	}
}
