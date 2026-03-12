using System;
using Sandbox;

namespace Facepunch.BombRoyale;

/// <summary>
/// Spawns explosion cloud particles along a line between two points,
/// with yellow spark/ember sprites flying off.
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

	private const int CloudCount = 20;
	private const int SparkCount = 50;
	private const float EffectLifetime = 2f;
	private const float BaseScale = 0.5f;

	private struct CloudParticle
	{
		public SceneObject SceneObject;
		public float BornTime;
		public float Lifetime;
		public Angles SpinRate;
		public Angles CurrentAngles;
		public float InitialScale;
	}

	private struct SparkParticle
	{
		public SceneObject SceneObject;
		public float BornTime;
		public float Lifetime;
		public Vector3 Velocity;
	}

	private CloudParticle[] _clouds;
	private SparkParticle[] _sparks;

	protected override void OnEnabled()
	{
		SpawnClouds();
		SpawnSparks();
	}

	private void SpawnClouds()
	{
		_clouds = new CloudParticle[CloudCount];
		var model = Model.Load( "models/particles/explosion/explosioncloud.vmdl" );

		for ( var i = 0; i < CloudCount; i++ )
		{
			var t = (float)i / (CloudCount - 1);
			var position = Vector3.Lerp( StartPosition, EndPosition, t );

			var so = new SceneObject( Scene.SceneWorld, model )
			{
				Transform = new Transform( position, Rotation.Random, 1f ),
				RenderingEnabled = true
			};

			_clouds[i] = new CloudParticle
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

	private void SpawnSparks()
	{
		_sparks = new SparkParticle[SparkCount];
		var sparkModel = Model.Load( "models/dev/sphere.vmdl" );

		for ( var i = 0; i < SparkCount; i++ )
		{
			var t = Game.Random.Float( 0f, 1f );
			var position = Vector3.Lerp( StartPosition, EndPosition, t );
			position += Vector3.Random.Normal * 12f;

			var so = new SceneObject( Scene.SceneWorld, sparkModel )
			{
				Transform = new Transform( position, Rotation.Identity, Game.Random.Float( 0.01f, 0.03f ) ),
				RenderingEnabled = true,
				ColorTint = new Color( 1f, 0.85f, 0.2f )
			};

			_sparks[i] = new SparkParticle
			{
				SceneObject = so,
				BornTime = Time.Now,
				Lifetime = Game.Random.Float( 1f, 2f ),
				Velocity = new Vector3(
					Game.Random.Float( -50f, 50f ),
					Game.Random.Float( -50f, 50f ),
					Game.Random.Float( 20f, 80f )
				)
			};
		}
	}

	protected override void OnPreRender()
	{
		UpdateClouds();
		UpdateSparks();
	}

	private void UpdateClouds()
	{
		if ( _clouds == null ) return;

		for ( var i = 0; i < _clouds.Length; i++ )
		{
			ref var p = ref _clouds[i];
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

	private void UpdateSparks()
	{
		if ( _sparks == null ) return;

		var dt = Time.Delta;
		const float gravity = -150f;

		for ( var i = 0; i < _sparks.Length; i++ )
		{
			ref var s = ref _sparks[i];
			if ( !s.SceneObject.IsValid() ) continue;

			var age = Time.Now - s.BornTime;
			var normalizedAge = age / s.Lifetime;

			if ( normalizedAge >= 1f )
			{
				s.SceneObject.Delete();
				continue;
			}

			s.Velocity += Vector3.Up * gravity * dt;
			var pos = s.SceneObject.Transform.Position + s.Velocity * dt;

			var alpha = 1f - normalizedAge;
			var color = Color.Lerp( new Color( 1f, 1f, 0.8f ), new Color( 1f, 0.5f, 0f ), normalizedAge );

			s.SceneObject.Transform = new Transform( pos, Rotation.Identity, s.SceneObject.Transform.Scale );
			s.SceneObject.ColorTint = color.WithAlpha( alpha );
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

	protected override void OnDisabled() => Cleanup();
	protected override void OnDestroy() => Cleanup();

	private void Cleanup()
	{
		if ( _clouds != null )
		{
			for ( var i = 0; i < _clouds.Length; i++ )
			{
				if ( _clouds[i].SceneObject.IsValid() )
					_clouds[i].SceneObject.Delete();
			}
			_clouds = null;
		}

		if ( _sparks != null )
		{
			for ( var i = 0; i < _sparks.Length; i++ )
			{
				if ( _sparks[i].SceneObject.IsValid() )
					_sparks[i].SceneObject.Delete();
			}
			_sparks = null;
		}
	}
}
