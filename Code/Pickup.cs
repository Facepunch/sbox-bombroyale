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

public abstract class Pickup : Component, IRestartable, Component.ICollisionListener
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

		Sound.Play( SpawnSound, Transform.Position );
		
		Effect = new( Scene.SceneWorld, "particles/gameplay/idle_coin/idle_coin.vpcf" );
		Effect.SetControlPoint( 0, Transform.Position );
		Effect.SetNamedValue( "color", Color * 255f );
		
		base.OnStart();
	}

	void ICollisionListener.OnCollisionStart( Collision other )
	{
		/*
		if ( Game.IsServer && other is BombRoyalePlayer player )
		{
			var fx = Particles.Create( "particles/gameplay/player/collectpickup/collectpickup.vpcf", Position );
			fx.Set( "color", Color * 255f );

			if ( !string.IsNullOrEmpty( PickupSound ) )
			{
				Sound.FromScreen( To.Single( player ), PickupSound );
			}

			OnPickup( player );
			Delete();
		}
		*/
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

	[Broadcast]
	private void PlaySound( string soundName, Vector3 position )
	{
		Sound.Play( soundName, position );
	}
}
