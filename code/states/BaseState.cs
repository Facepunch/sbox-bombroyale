using Sandbox;

namespace Facepunch.BombRoyale;

public class BaseState : BaseNetworkable
{
	public StateSystem System { get; set; }

	public virtual int TimeLeft => 0;
	public virtual bool IsPaused => false;
	public virtual string Name => string.Empty;

	public virtual void OnEnter() { }

	public virtual void OnLeave() { }

	public virtual bool CanHearPlayerVoice( IClient a, IClient b )
	{
		return false;
	}

	public virtual void OnPlayerKilled( BombRoyalePlayer player, DamageInfo info ) { }

	public virtual void OnPlayerJoined( BombRoyalePlayer player ) { }

	public virtual void OnPlayerRespawned( BombRoyalePlayer player ) { }

	public virtual void OnPlayerDisconnected( BombRoyalePlayer player ) { }
}
