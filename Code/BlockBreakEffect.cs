using Sandbox;

namespace Facepunch.BombRoyale;

/// <summary>
/// Spawns debris model particles bursting outward when a block is destroyed.
/// </summary>
[Title( "Block Break Effect" )]
[Category( "Bomb Royale" )]
public class BlockBreakEffect : Component
{
	public static void Create( Scene scene, Vector3 position )
	{
		var go = new GameObject( false, "BlockBreak" )
		{
			WorldPosition = position
		};

		var effect = go.AddComponent<BlockBreakEffect>();
		effect.SpawnPosition = position;

		go.AddComponent<TemporaryEffect>().DestroyAfterSeconds = 2f;
		go.Enabled = true;
	}

	private Vector3 SpawnPosition { get; set; }

	private const int ParticleCount = 5;
	private const float Gravity = -200f;

	private struct DebrisParticle
	{
		public SceneObject SceneObject;
		public Vector3 Velocity;
		public float BornTime;
		public Angles SpinRate;
		public Angles CurrentAngles;
	}

	private DebrisParticle[] _particles;

	protected override void OnEnabled()
	{
		_particles = new DebrisParticle[ParticleCount];
		var model = Model.Load( "models/particles/debris/debris_a.vmdl" );

		for ( var i = 0; i < ParticleCount; i++ )
		{
			var offset = Vector3.Random.Normal * 16f;
			var position = SpawnPosition + offset;
			var velocity = (offset.Normal + Vector3.Up * 0.5f) * Game.Random.Float( 50f, 150f );

			var so = new SceneObject( Scene.SceneWorld, model )
			{
				Transform = new Transform( position, Rotation.Random, Game.Random.Float( 2.5f, 3.1f ) ),
				ColorTint = new Color( 0.682f, 0.467f, 0.361f )
			};

			_particles[i] = new DebrisParticle
			{
				SceneObject = so,
				Velocity = velocity,
				BornTime = Time.Now,
				SpinRate = new Angles(
					Game.Random.Float( -180f, 180f ),
					Game.Random.Float( -180f, 180f ),
					Game.Random.Float( -180f, 180f )
				),
				CurrentAngles = new Angles(
					Game.Random.Float( 0f, 360f ),
					Game.Random.Float( 0f, 360f ),
					Game.Random.Float( 0f, 360f )
				)
			};
		}
	}

	protected override void OnPreRender()
	{
		if ( _particles == null ) return;

		var dt = Time.Delta;

		for ( var i = 0; i < _particles.Length; i++ )
		{
			ref var p = ref _particles[i];
			if ( !p.SceneObject.IsValid() ) continue;

			var age = Time.Now - p.BornTime;

			if ( age > 1.5f )
			{
				p.SceneObject.Delete();
				continue;
			}

			p.Velocity += Vector3.Up * Gravity * dt;
			var pos = p.SceneObject.Transform.Position + p.Velocity * dt;
			p.CurrentAngles += p.SpinRate * dt;

			var alpha = 1f;
			if ( age > 0.1f )
				alpha = 1f - ((age - 0.1f) / 1.4f);

			p.SceneObject.Transform = new Transform( pos, p.CurrentAngles.ToRotation(), p.SceneObject.Transform.Scale );
			p.SceneObject.ColorTint = new Color( 0.682f, 0.467f, 0.361f, alpha.Clamp( 0f, 1f ) );
		}
	}

	protected override void OnDisabled() => CleanupParticles();
	protected override void OnDestroy() => CleanupParticles();

	private void CleanupParticles()
	{
		if ( _particles == null ) return;

		for ( var i = 0; i < _particles.Length; i++ )
		{
			if ( _particles[i].SceneObject.IsValid() )
				_particles[i].SceneObject.Delete();
		}

		_particles = null;
	}
}
