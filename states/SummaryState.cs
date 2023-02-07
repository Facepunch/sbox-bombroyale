using Sandbox;
using System.Linq;

namespace Facepunch.BombsAway;

public partial class SummaryState : BaseState
{
	[Net] public RealTimeUntil StateEndTime { get; private set; }

	public override void OnEnter()
	{
		if ( Game.IsServer )
		{
			StateEndTime = 10f;
		}
	}

	public override void OnLeave()
	{
		base.OnLeave();
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
