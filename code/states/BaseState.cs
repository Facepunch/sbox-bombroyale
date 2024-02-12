﻿using Sandbox;

namespace Facepunch.BombRoyale;

public abstract class BaseState : Component
{
	public virtual int TimeLeft => 0;
	public virtual bool IsPaused => false;
	public virtual string Name => string.Empty;

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
		StateSystem.Active = null;
		OnLeave();
		base.OnDestroy();
	}
}
