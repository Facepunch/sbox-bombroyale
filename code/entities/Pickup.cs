using Sandbox;
using System;
using System.Linq;

namespace Facepunch.BombRoyale;

public class PickupChanceAttribute : Attribute
{
	public float Chance { get; set; } = 0.5f;
	
	public PickupChanceAttribute( float chance )
	{
		Chance = chance;
	}
}

public abstract class Pickup : ModelEntity
{
	public static Pickup CreateRandom()
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
				return type.Create<Pickup>();
			}
		}

		return default;
	}

	public virtual string PickupSound => null;
	public virtual string SpawnSound => null;
	public virtual Color Color => Color.Orange;
	public virtual string Icon => null;

	private PointLightEntity Light { get; set; }
	private PickupSprite Sprite { get; set; }
	private Particles Effect { get; set; }

	public override void Spawn()
	{
		EnableTouch = true;
		Transmit = TransmitType.Always;

		SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero, 8f );

		Tags.Add( "pickup" );

		if ( !string.IsNullOrEmpty( SpawnSound ) )
		{
			PlaySound( SpawnSound );
		}

		base.Spawn();
	}

	public override void ClientSpawn()
	{
		Sprite = new( this );

		Light = new();
		Light.SetParent( this );
		Light.Position = Position;
		Light.Brightness = 0.1f;
		Light.Range = 40f;
		Light.Color = Color;

		Effect = Particles.Create( "particles/gameplay/idle_coin/idle_coin.vpcf", this );
		Effect.Set( "color", Color * 255f );

		base.ClientSpawn();
	}

	public override void StartTouch( Entity other )
	{
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

		base.StartTouch( other );
	}

	protected virtual void OnPickup( BombRoyalePlayer player )
	{

	}

	protected override void OnDestroy()
	{
		Effect?.Destroy();
		Sprite?.Delete();

		base.OnDestroy();
	}
}
