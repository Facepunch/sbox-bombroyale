﻿using System;
using System.Linq;
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Diagnostics;

namespace Facepunch.BombRoyale;

public class Player : Component, IHealthComponent
{
	public static Player Me { get; private set; }

	[HostSync] public LifeState LifeState { get; private set; } = LifeState.Dead;
	[HostSync] public float MaxHealth { get; private set; } = 100f;
	[HostSync] public float Health { get; private set; }
	[HostSync] public int PlayerSlot { get; set; }
	
	[HostSync] public TimeSince LastTakeDamageTime { get; private set; }
	[HostSync] public DiseaseType Disease { get; set; } = DiseaseType.None;
	[HostSync] public TimeUntil RemoveDiseaseTime { get; set; }
	[HostSync] public bool HasSuperBomb { get; set; }
	[HostSync] public int SpeedBoosts { get; set; }
	[HostSync] public int LivesLeft { get; set; }
	[HostSync] public int BombRange { get; set; }
	[HostSync] public int MaxBombs { get; set; }
	
	[HostSync] private Guid HoldingBombId { get; set; }
	public Bomb HoldingBomb => Scene.Directory.FindComponentByGuid( HoldingBombId ) as Bomb;
	
	private TimeUntil NextRandomTeleport { get; set; }
	private TimeUntil NextRandomBomb { get; set; }
	private CitizenAnimationHelper Animation { get; set; }
	private CharacterController Controller { get; set; }
	private SkinnedModelRenderer Renderer { get; set; }
	private Vector2 InputDirection { get; set; }
	[Sync] private Vector3 WishVelocity { get; set; }
	
	private static readonly Color[] Colors = new Color[4]
	{
		"#F6D953",
		"#DB3D76",
		"#3DBFDB",
		"#FF881B"
	};
	
	private readonly Vector3[] Cardinals = {
		Vector3.Forward,
		Vector3.Left,
		Vector3.Right,
		Vector3.Backward
	};
	
	public Color GetTeamColor()
	{
		return Colors[PlayerSlot];
	}
	
	[Broadcast( NetPermission.HostOnly )]
	public void Respawn()
	{
		ShowRespawnEffect();

		if ( Networking.IsHost )
		{
			LifeState = LifeState.Alive;
			SpeedBoosts = 0;
			LivesLeft = 1;
			BombRange = 2;
			Disease = DiseaseType.None;
			MaxBombs = 1; 
			Health = MaxHealth;
		}

		if ( !IsProxy )
		{
			Controller.Velocity = Vector3.Zero;
			MoveToSpawnpoint();
		}
	}

	public void MoveToSpawnpoint()
	{
		var spawnpoints = Scene.GetAllComponents<PlayerSpawn>().ToList();
		spawnpoints.Sort( ( a, b ) => a.Index.CompareTo( b.Index ) );

		var spawnpoint = spawnpoints[PlayerSlot];
		if ( !spawnpoint.IsValid() )
		{
			throw new( $"Can't find spawnpoint for player slot #{PlayerSlot}" );
		}
		
		Transform.Position = spawnpoint.Transform.Position;
		Transform.Rotation = spawnpoint.Transform.Rotation;
	}
	
	public int GetBombsLeft() => MaxBombs - GetPlacedBombCount();

	public int GetPlacedBombCount()
	{
		return !Scene.IsValid() ? 0 : Scene.GetAllComponents<Bomb>().Count( b => b.Player == this && b.IsPlaced );
	}
	
	[Broadcast]
	public void TakeDamage( DamageType type, float damage, Vector3 position, Vector3 force, Guid attackerId )
	{
		
	}
	
	protected override void OnAwake()
	{
		Controller = Components.Get<CharacterController>( true );
		Renderer = Components.Get<SkinnedModelRenderer>( true );
		Animation = Components.Get<CitizenAnimationHelper>( true );
		
		base.OnAwake();
	}

	protected override void OnStart()
	{
		if ( Network.IsOwner ) Me = this;

		if ( !Networking.IsHost )
		{
			BombRoyale.AddPlayer( PlayerSlot, this );
		}
		
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		UpdateLifeState();
		UpdateAnimation();
		
		if ( IsProxy ) return;

		UpdateCamera();
		UpdateMovement();
	}

	private void UpdateLifeState()
	{
		var isAlive = LifeState == LifeState.Alive;
		
		Controller.Enabled = isAlive;
		Renderer.Enabled = isAlive;

		var children = Components.GetAll<ModelRenderer>( FindMode.EverythingInDescendants );
		foreach ( var child in children )
		{
			child.Enabled = isAlive;
		}
	}
	
	private void ShowRespawnEffect()
	{
		var fx = new SceneParticles( Scene.SceneWorld, "particles/gameplay/player/respawn/respawn_effect.vpcf" );
		fx.SetControlPoint( 0, Transform.Position );
		fx.SetNamedValue( "Color", GetTeamColor() * 255f );
		fx.PlayUntilFinished( Task );
		
		Sound.Play( "player.teleport", Transform.Position );
	}

	private void UpdateAnimation()
	{
		var animator = Animation;
		animator.HoldType = CitizenAnimationHelper.HoldTypes.None;
		animator.WithVelocity( Controller.Velocity );
		animator.WithWishVelocity( WishVelocity );
		animator.IsGrounded = true;
		animator.FootShuffle = 0f;
		animator.DuckLevel = 0f;
		animator.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
	}
	
	private Vector3 SnapInputDirection( Vector3 direction )
	{
		if ( direction.Length == 0f )
			return direction;

		var output = Vector3.Zero;
		var ldp = -1f;

		foreach ( var c in Cardinals )
		{
			var dp = direction.Dot( c );
			if ( dp > ldp )
			{
				output = c;
				ldp = dp;
			}
		}

		return output;
	}
	
	private float GetWishSpeed()
	{
		const float walkSpeed = 150f;

		return Disease switch
		{
			DiseaseType.MoveFast => walkSpeed * 1.75f,
			DiseaseType.MoveSlow => walkSpeed * 0.75f,
			_ => walkSpeed + (25f * SpeedBoosts)
		};
	}
	
	private void UpdateMovement()
	{
		InputDirection = SnapInputDirection( Input.AnalogMove );
		
		WishVelocity = new( InputDirection.x, InputDirection.y, 0f );
		WishVelocity = WishVelocity.WithZ( 0f );
		WishVelocity *= GetWishSpeed();
		
		Controller.Velocity = Controller.Velocity.WithZ( 0f );
		Controller.Accelerate( WishVelocity );
		Controller.ApplyFriction( 4f );

		Controller.Move();
		
		if ( InputDirection.Length > 0f )
		{
			Transform.Rotation = Rotation.Lerp( Transform.Rotation, Rotation.LookAt( InputDirection, Vector3.Up ),
				Time.Delta * 16f );
		}
	}

	private void UpdateCamera()
	{
		var boundsComponent = Scene.GetAllComponents<ArenaBounds>().FirstOrDefault();
		var worldBounds = boundsComponent.Bounds;
		var camera = Scene.Camera;
		
		var totalHeight = worldBounds.Size.Length;
		camera.Transform.Position = worldBounds.Center + Vector3.Up * totalHeight * 0.85f + Vector3.Backward * totalHeight * 0.15f;
		var direction = (worldBounds.Center - camera.Transform.Position).Normal - Vector3.Backward *.002f;
		camera.Transform.Rotation = Rotation.LookAt( direction );

		camera.FieldOfView = Screen.CreateVerticalFieldOfView( 60f );

		ScreenShake.Apply( camera );

		Sound.Listener = new( Transform.Position + Vector3.Up * 60f, Rotation.LookAt( Vector3.Forward ) );
	}
}
