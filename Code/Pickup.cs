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
		go.WorldPosition = position;
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
	private PickupIdleEffect IdleEffect { get; set; }

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
			Sound.Play( SpawnSound, WorldPosition );
		
		IdleEffect = Components.Create<PickupIdleEffect>();
		IdleEffect.EffectColor = Color;

		base.OnStart();
	}

	void ITriggerListener.OnTriggerEnter( Collider collider )
	{
		if ( !Networking.IsHost ) return;

		var player = collider.Components.GetInAncestorsOrSelf<Player>();
		if ( !player.IsValid() ) return;

		if ( !OnPickup( player ) )
			return;
		
		DoPickupEffects();
		GameObject.Destroy();
	}

	protected virtual bool OnPickup( Player player )
	{
		return false;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void DoPickupEffects()
	{
		CollectPickupEffect.Create( Scene, WorldPosition, Color );

		if ( !string.IsNullOrEmpty( PickupSound ) )
		{
			Sound.Play( PickupSound, WorldPosition );
		}
	}

	[Rpc.Broadcast]
	private void PlaySound( string soundName, Vector3 position )
	{
		Sound.Play( soundName, position );
	}
}
