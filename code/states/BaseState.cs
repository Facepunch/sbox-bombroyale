using Sandbox;

namespace Facepunch.BombsAway;

public class BaseState : BaseNetworkable
{
	public StateSystem System { get; set; }

	public virtual void OnEnter() { }

	public virtual void OnLeave() { }

	public virtual bool CanHearPlayerVoice( IClient a, IClient b )
	{
		return false;
	}

	public virtual void OnPlayerKilled( BombsAwayPlayer player, DamageInfo info ) { }

	public virtual void OnPlayerJoined( BombsAwayPlayer player ) { }

	public virtual void OnPlayerRespawned( BombsAwayPlayer player ) { }

	public virtual void OnPlayerDisconnected( BombsAwayPlayer player ) { }
}
