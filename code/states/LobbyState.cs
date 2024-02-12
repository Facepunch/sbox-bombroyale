﻿using Sandbox;

namespace Facepunch.BombRoyale;

public class LobbyState : BaseState
{
	public override string Name => "WAIT";
	public override int TimeLeft => RoundEndTime.Relative.CeilToInt();

	[Sync] public TimeUntil RoundEndTime { get; set; }

	private bool PlayedCountdown { get; set; }
	private SoundHandle Countdown { get; set; }

	protected override void OnEnter()
	{
		if ( Networking.IsHost )
		{
			IResettable.ResetAll();
			RoundEndTime = 10f;
		}
	}

	protected override void OnLeave()
	{
		Countdown?.Stop();
		Countdown = null;
	}

	protected override void OnUpdate()
	{
		if ( Networking.IsHost )
		{
			if ( RoundEndTime )
			{
				StateSystem.Set<GameState>();
				return;
			}
			
			if ( RoundEndTime > 5f && Networking.Connections.Count == 4 )
			{
				RoundEndTime = 5f;
			}
		}
		
		if ( RoundEndTime <= 5f && !PlayedCountdown )
		{
			PlayedCountdown = true;
			Countdown = Sound.Play( "round.countdown" );
		}
		
		base.OnUpdate();
	}
}