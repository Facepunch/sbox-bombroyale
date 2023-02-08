using Sandbox;

namespace Facepunch.BombRoyale;

public partial class StateSystem : BaseNetworkable
{
	[Net, Change( nameof( OnStateChanged ) )] public BaseState Active { get; private set; }

	public void Set( BaseState state )
	{
		Game.AssertServer();

		if ( Active != null )
		{
			Active.OnLeave();
			Event.Unregister( Active );
		}

		Active = state;
		Active.System = this;
		Active?.OnEnter();

		Event.Register( Active );
	}

	private void OnStateChanged( BaseState oldState, BaseState newState )
	{
		if ( oldState != null )
		{
			oldState.OnLeave();
			Event.Unregister( oldState );
		}

		newState.System = this;
		newState.OnEnter();

		Event.Register( newState );
	}
}
