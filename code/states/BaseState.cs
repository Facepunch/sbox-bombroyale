using Sandbox;

namespace Facepunch.BombRoyale;

public abstract class BaseState : Component
{
	public virtual int TimeLeft => 0;
	public virtual string Name => string.Empty;
	public virtual bool IsPaused => false;

	protected virtual void OnEnter() { }

	protected virtual void OnLeave() { }

	protected override void OnAwake()
	{
		StateSystem.Active = this;
		OnEnter();
		
		base.OnAwake();
	}

	protected override void OnDestroy()
	{
		if ( StateSystem.Active == this )
			StateSystem.Active = null;
		
		OnLeave();
		
		base.OnDestroy();
	}
}
