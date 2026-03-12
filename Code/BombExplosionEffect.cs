using System;
using Sandbox;

namespace Facepunch.BombRoyale;

/// <summary>
/// Spawns explosion cloud particles along a line between two points.
/// </summary>
[Title( "Bomb Explosion Effect" )]
[Category( "Bomb Royale" )]
public class BombExplosionEffect : Component
{
	private static readonly Gradient FireGradient = new Gradient(
		new Gradient.ColorFrame( 0.000f, Color.White ),
		new Gradient.ColorFrame( 0.121f, new Color( 1.0f, 0.816f, 0.0f ) ),
		new Gradient.ColorFrame( 0.230f, new Color( 1.0f, 0.616f, 0.145f ) ),
		new Gradient.ColorFrame( 0.437f, new Color( 1.0f, 0.231f, 0.0f ) ),
		new Gradient.ColorFrame( 1.000f, new Color( 0.165f, 0.035f, 0.0f ) )
	);

	public static void Create( Scene scene, Vector3 startPosition, Vector3 endPosition )
	{
		var go = new GameObject( false, "BombExplosion" )
		{
			WorldPosition = startPosition
		};

		var effect = go.AddComponent<BombExplosionEffect>();
		effect.StartPosition = startPosition;
		effect.EndPosition = endPosition;

		go.AddComponent<TemporaryEffect>().DestroyAfterSeconds = 3f;
		go.Enabled = true;
	}

	private Vector3 StartPosition { get; set; }
	private Vector3 EndPosition { get; set; }

	private const int ParticleCount = 20;
	private const float EffectLifetime = 2f;
	private const float BaseScale = 0.5f;

	private struct ExplosionParticle
	{
		public SceneObject SceneObject;
		public float BornTime;
		public float Lifetime;
		public Angles SpinRate;
		public Angles CurrentAngles;
		public float InitialScale;
	}

	private ExplosionParticle[] _particles;

	protected override void OnEnabled()
	{
		_particles = new ExplosionParticle[ParticleCount];
		var model = Model.Load( "models/particles/explosion/explosioncloud.vmdl" );

		for ( var i = 0; i < ParticleCount; i++ )
		{
			var t = (float)i / (ParticleCount - 1);
			var position = Vector3.Lerp( StartPosition, EndPosition, t );

			var so = new SceneObject( Scene.SceneWorld, model )
			{
				Transform = new Transform( position, Rotation.Random, 1f ),
				RenderingEnabled = true
			};

			_particles[i] = new ExplosionParticle
			{
				SceneObject = so,
				BornTime = Time.Now,
				Lifetime = EffectLifetime,
				SpinRate = new Angles(
					Game.Random.Float( -30f, 30f ),
					Game.Random.Float( -30f, 30f ),
					Game.Random.Float( -30f, 30f )
				),
				CurrentAngles = new Angles(
					Game.Random.Float( -360f, 360f ),
					Game.Random.Float( -360f, 360f ),
					Game.Random.Float( -360f, 360f )
				),
				InitialScale = Game.Random.Float( 0.5f, 1.0f ) * BaseScale
			};
		}
	}

	protected override void OnPreRender()
	{
		if ( _particles == null ) return;

		for ( var i = 0; i < _particles.Length; i++ )
		{
			ref var p = ref _particles[i];
			if ( !p.SceneObject.IsValid() ) continue;

			var age = Time.Now - p.BornTime;
			var normalizedAge = age / p.Lifetime;

			if ( normalizedAge >= 1f )
			{
				p.SceneObject.Delete();
				continue;
			}

			var sizeCurve = EvaluateSizeCurve( age );
			var scale = p.InitialScale * sizeCurve;

			p.CurrentAngles += p.SpinRate * Time.Delta;

			var color = FireGradient.Evaluate( normalizedAge );

			const float fadeStart = 0.25f;
			var alpha = 1f;

			if ( normalizedAge > fadeStart )
				alpha = 1f - ((normalizedAge - fadeStart) / (1f - fadeStart));

			p.SceneObject.Transform = new Transform( p.SceneObject.Transform.Position, p.CurrentAngles.ToRotation(), scale );
			p.SceneObject.ColorTint = color.WithAlpha( alpha );
		}
	}

	private static float EvaluateSizeCurve( float age )
	{
		if ( age <= 0f ) return 0f;
		if ( age >= EffectLifetime ) return 0f;

		if ( age < 0.05f )
			return (age / 0.05f) * 0.7f;

		var t = (age - 0.05f) / (EffectLifetime - 0.05f);
		return MathX.Lerp( 0.7f, 0f, t * t );
	}

	protected override void OnDisabled()
	{
		CleanupParticles();
	}

	protected override void OnDestroy()
	{
		CleanupParticles();
	}

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
