using Sandbox;
using System;
using System.Linq;

namespace Facepunch.BombRoyale;

public class PickupChanceAttribute : Attribute
{
	public float Chance { get; set; }
	
	public PickupChanceAttribute( float chance )
	{
		Chance = chance;
	}
}

public abstract class Pickup : Component, IRestartable, Component.ITriggerListener
{
	public static Pickup CreateRandom( Vector3 position )
	{
		var possibleTypes = TypeLibrary.GetTypes<Pickup>()
			.Where( t => !t.IsAbstract )
			.Where( t => t.HasAttribute<PickupChanceAttribute>() )
			.Shuffle()
			.ToList();

		if ( !possibleTypes.Any() )
			return default;

		var u = possibleTypes.Sum( p => p.GetAttribute<PickupChanceAttribute>().Chance );
		var r = Game.Random.Float() * u;
		var s = 0f;

		foreach ( var type in possibleTypes )
		{
			var chance = type.GetAttribute<PickupChanceAttribute>().Chance;

			s += chance;

			if ( r < s )
			{
				return CreatePickup( type, position );
			}
		}

		return default;
	}

	private static Pickup CreatePickup( TypeDescription type, Vector3 position )
	{
		var go = BombRoyale.Instance.PickupPrefab.Clone();
		var pickup = go.Components.Create( type ) as Pickup;
		go.Transform.Position = position;
		go.Name = type.ClassName;
		go.NetworkSpawn();
		return pickup;
	}

	public virtual string PickupSound => null;
	public virtual string SpawnSound => null;
	public virtual Color Color => Color.Orange;
	public virtual string Icon => null;

	private PointLight Light { get; set; }
	private PickupSprite Sprite { get; set; }
	private SceneParticles Effect { get; set; }

	void IRestartable.OnRestart()
	{
		GameObject.Destroy();
	}

	protected override void OnStart()
	{
		Light = Components.Get<PointLight>();
		Light.LightColor = Color;
		
		Sprite = Components.GetInDescendantsOrSelf<PickupSprite>();
		Sprite.Pickup = this;

		if ( !string.IsNullOrEmpty( SpawnSound ) )
			Sound.Play( SpawnSound, Transform.Position );
		
		Effect = new( Scene.SceneWorld, "particles/gameplay/idle_coin/idle_coin.vpcf" );
		Effect.SetControlPoint( 0, Transform.Position );
		Effect.SetNamedValue( "color", Color * 255f );
		
		base.OnStart();
	}

	void ITriggerListener.OnTriggerEnter( Collider collider )
	{
		if ( !Networking.IsHost ) return;

		var player = collider.Components.GetInAncestorsOrSelf<Player>();
		if ( !player.IsValid() ) return;

		DoPickupEffects();
		OnPickup( player );
		
		GameObject.Destroy();
	}

	protected virtual void OnPickup( Player player )
	{

	}

	protected override void OnUpdate()
	{
		if ( Effect.IsValid() )
		{
			Effect.SetControlPoint( 0, Transform.Position );
			Effect.Simulate( Time.Delta );
		}
		
		base.OnUpdate();
	}

	protected override void OnDestroy()
	{
		Effect?.Delete();
		Effect = null;
		
		base.OnDestroy();
	}

	[Broadcast( NetPermission.HostOnly )]
	private void DoPickupEffects()
	{
		var fx = new SceneParticles( Scene.SceneWorld, "particles/gameplay/player/collectpickup/collectpickup.vpcf" );
		fx.SetControlPoint( 0, Transform.Position );
		fx.SetNamedValue( "color", Color * 255f );
		fx.PlayUntilFinished();

		if ( !string.IsNullOrEmpty( PickupSound ) )
		{
			Sound.Play( PickupSound, Transform.Position );
		}
	}

	[Broadcast]
	private void PlaySound( string soundName, Vector3 position )
	{
		Sound.Play( soundName, position );
	}
}
