using Sandbox;
using System.Linq;

namespace Facepunch.BombRoyale;

public abstract class Pickup : ModelEntity
{
	public static Pickup CreateRandom()
	{
		var types = TypeLibrary.GetTypes<Pickup>()
			.Where( t => !t.IsAbstract )
			.ToList();

		var type = Game.Random.FromList( types );
		return type.Create<Pickup>();
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
		Transmit = TransmitType.Always;
		EnableTouch = true;

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

		base.ClientSpawn();
	}

	public override void StartTouch( Entity other )
	{
		if ( Game.IsServer && other is BombRoyalePlayer player )
		{
			Particles.Create( "particles/gameplay/player/collectpickup/collectpickup.vpcf", Position );

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
