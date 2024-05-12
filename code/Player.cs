using System;
using System.Linq;
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Diagnostics;

namespace Facepunch.BombRoyale;

public class Player : Component, IHealthComponent, Component.ICollisionListener
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
	private DiseaseSprite DiseaseSprite { get; set; }
	private Vector2 InputDirection { get; set; }
	[Sync] private Vector3 WishVelocity { get; set; }
	
	[Property] public CitizenAnimationHelper Animation { get; set; }
	[Property] public MoveController Controller { get; set; }
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public RagdollController Ragdoll { get; set; }
	[Property] public GameObject BombPrefab { get; set; }
	
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
			ShowRespawnEffect( Transform.Position );
		}
	}
	
	public bool IsInsideBomb()
	{
		var trace = Scene.Trace.Ray( Transform.Position, Transform.Position )
			.Size( Controller.BoundingBox.Size )
			.IgnoreGameObject( GameObject )
			.WithTag( "bomb" )
			.Run();
		
		return trace.GameObject?.Components.Get<Bomb>() is not null;
	}
	
	public bool IsInsideBomb( Bomb bomb )
	{
		var trace = Scene.Trace.Ray( Transform.Position, Transform.Position )
			.Size( Controller.BoundingBox.Size )
			.IgnoreGameObject( GameObject )
			.WithTag( "bomb" )
			.Run();
		
		return trace.GameObject.IsValid() && trace.GameObject.Components.Get<Bomb>() == bomb;
	}

	public void SetHoldingBomb( Bomb bomb )
	{
		Assert.True( Networking.IsHost );
		HoldingBombId = bomb.Id;
	}

	public void MoveToSpawnpoint()
	{
		var spawnpoints = Scene.GetAllComponents<PlayerSpawn>().ToList();
		spawnpoints.Sort( ( a, b ) => a.Index.CompareTo( b.Index ) );

		var spawnpoint = spawnpoints[PlayerSlot];
		if ( !spawnpoint.IsValid() )
			throw new( $"Can't find spawnpoint for player slot #{PlayerSlot}" );
		
		Transform.Position = spawnpoint.Transform.Position;
		Transform.Rotation = spawnpoint.Transform.Rotation;
	}
	
	public void GiveDisease( DiseaseType disease )
	{
		Assert.True( Networking.IsHost );
		
		RemoveDiseaseTime = Game.Random.Float( 10f, 20f );
		Disease = disease;

		if ( disease == DiseaseType.RandomBomb )
			NextRandomBomb = Game.Random.Float( 1f, 2f );
		else if ( disease == DiseaseType.Teleport )
			NextRandomTeleport = Game.Random.Float( 0.5f, 1f );

		Chat.AddPlayerEvent( "infected", Network.OwnerConnection.DisplayName, GetTeamColor(), $"has been infected with {disease.GetName()}" );
	}
	
	public int GetBombsLeft() => MaxBombs - GetPlacedBombCount();

	public int GetPlacedBombCount()
	{
		return !Scene.IsValid() ? 0 : Scene.GetAllComponents<Bomb>().Count( b => b.Player == this && b.IsPlaced );
	}
	
	public void TakeDamage( DamageType type, float damage, Vector3 position, Vector3 force, Guid attackerId )
	{
		Assert.True( Networking.IsHost );
		
		if ( LifeState == LifeState.Dead ) return;
		if ( type != DamageType.Explosion ) return;

		LastTakeDamageTime = 0f;
		LivesLeft--;

		if ( LivesLeft <= 0 )
		{
			using ( Rpc.FilterInclude( Network.OwnerConnection ) )
			{
				PlaySound( "player.die" );
			}
			
			LifeState = LifeState.Dead;
			
			var direction = Vector3.Up + new Vector3( Game.Random.Float( -0.25f, 0.25f ), Game.Random.Float( -0.25f, 0.25f ), 0f );
			Ragdoll.Ragdoll( position, direction );

			Chat.AddPlayerEvent( "death", Network.OwnerConnection.DisplayName, GetTeamColor(), "has been blown to smithereens!" );
		}
		else
		{
			using ( Rpc.FilterInclude( Network.OwnerConnection ) )
			{
				PlaySound( "lose.life" );
			}
		}
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

	protected override void OnDestroy()
	{
		if ( DiseaseSprite.IsValid() )
		{
			DiseaseSprite.GameObject.Destroy();
			DiseaseSprite = null;
		}
		
		base.OnDestroy();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( Networking.IsHost )
		{
			if ( Disease > DiseaseType.None && RemoveDiseaseTime )
			{
				Disease = DiseaseType.None;
			}
		}
		
		if ( Disease == DiseaseType.None && DiseaseSprite.IsValid() )
		{
			DiseaseSprite.GameObject.Destroy();
			DiseaseSprite = null;
			return;
		}

		if ( Disease > DiseaseType.None && !DiseaseSprite.IsValid() )
		{
			DiseaseSprite = DiseaseSprite.Create( this );
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		UpdateLifeState( LifeState );
		
		if ( LifeState == LifeState.Alive )
			UpdateAnimation();
		
		if ( IsProxy ) return;

		UpdateCamera();
		
		if ( BombRoyale.IsPaused )
		{
			Controller.Velocity = 0f;
			return;
		}

		if ( LifeState == LifeState.Dead )
			return;
		
		UpdateMovement();
	
		if ( Input.Released( "attack1" ) )
		{
			PlaceBombOnHost();
		}
	}

	[Broadcast( NetPermission.OwnerOnly )]
	private void PlaceBombOnHost()
	{
		if ( !Networking.IsHost )
			return;

		if ( IsInsideBomb() )
			return;
		
		if ( HoldingBomb.IsValid() )
		{
			PlaySound( "bomb.place" );
			HoldingBomb.Place( this );
			HoldingBombId = default;
		}
		else if ( GetBombsLeft() > 0 )
		{
			var bombGo = BombPrefab.Clone();
			var bomb = bombGo.Components.Get<Bomb>();
			bomb.Place( this );
			bombGo.NetworkSpawn();
			
			PlaySound( "bomb.place" );
		}
		else
		{
			using ( Rpc.FilterInclude( Network.OwnerConnection ) )
			{
				PlaySound( "bomb.nobomb" );
			}
		}
	}

	[Broadcast]
	private void PlaySound( string soundName )
	{
		Sound.Play( soundName, Transform.Position );
	}

	private void UpdateLifeState( LifeState state )
	{
		var isAlive = state == LifeState.Alive;
		
		Controller.Enabled = isAlive;
		Renderer.Enabled = Ragdoll.IsRagdolled || isAlive;

		var children = Components.GetAll<ModelRenderer>( FindMode.EverythingInDescendants );
		foreach ( var child in children )
		{
			child.Enabled = Ragdoll.IsRagdolled || isAlive;
		}
	}
	
	[Broadcast]
	private void ShowRespawnEffect( Vector3 position )
	{
		var fx = new SceneParticles( Scene.SceneWorld, "particles/gameplay/player/respawn/respawn_effect.vpcf" );
		fx.SetControlPoint( 0, position );
		fx.SetNamedValue( "Color", GetTeamColor() * 255f );
		fx.PlayUntilFinished( Task );
		
		Sound.Play( "player.teleport", position );
	}

	private void UpdateAnimation()
	{
		var animator = Animation;
		animator.HoldType = CitizenAnimationHelper.HoldTypes.None;
		animator.WithVelocity( Controller.Velocity );
		animator.WithWishVelocity( WishVelocity );
		animator.IsGrounded = true;
		animator.MoveRotationSpeed = 0f;
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

		// Store our previous up position.
		var previousZ = Transform.Position.z;

		Controller.Move();
		
		// Always make sure we never change our up position.
		Transform.Position = Transform.Position.WithZ( previousZ );
		
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
