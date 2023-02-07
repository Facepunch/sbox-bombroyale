using Sandbox;

namespace Facepunch.BombsAway;

public partial class LobbyState : BaseState
{
	[Net] public RealTimeUntil StateEndTime { get; set; }
	public float StateDuration => 15f;

	public override void OnEnter()
	{
		if ( Game.IsServer )
		{
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
