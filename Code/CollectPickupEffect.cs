using Sandbox;

namespace Facepunch.BombRoyale;

/// <summary>
/// Burst of particles when a pickup is collected.
/// Replaces particles/gameplay/player/collectpickup/collectpickup.vpcf
/// </summary>
[Title( "Collect Pickup Effect" )]
[Category( "Bomb Royale" )]
public class CollectPickupEffect : Component
{
	public static void Create( Scene scene, Vector3 position, Color color )
	{
		var go = new GameObject( false, "CollectPickup" )
		{
			WorldPosition = position
		};

		var effect = go.AddComponent<CollectPickupEffect>();
		effect.EffectColor = color;

		go.AddComponent<TemporaryEffect>().DestroyAfterSeconds = 2f;
		go.Enabled = true;
	}

	private Color EffectColor { get; set; } = Color.Orange;

	protected override void OnEnabled()
	{
		var effect = Components.GetOrCreate<ParticleEffect>();
		effect.MaxParticles = 20;
		effect.Lifetime = new ParticleFloat( 0.5f, 1.0f );
		effect.ApplyColor = true;
		effect.Tint = EffectColor;
		effect.ApplyAlpha = true;
		effect.Alpha = new ParticleFloat { Type = ParticleFloat.ValueType.Range, Evaluation = ParticleFloat.EvaluationType.Life, ConstantA = 1f, ConstantB = 0f };
		effect.ApplyShape = true;
		effect.Scale = new ParticleFloat { Type = ParticleFloat.ValueType.Range, Evaluation = ParticleFloat.EvaluationType.Life, ConstantA = 1.5f, ConstantB = 0f };
		effect.Force = true;
		effect.ForceDirection = new Vector3( 0, 0, -150f );
		effect.Damping = 2f;

		var emitter = Components.GetOrCreate<ParticleSphereEmitter>();
		emitter.Radius = 5f;
		emitter.Velocity = 80f;
		emitter.Loop = false;
		emitter.Duration = 0.1f;
		emitter.Burst = 20;
		emitter.Rate = 0;

		var renderer = Components.GetOrCreate<ParticleSpriteRenderer>();
		renderer.Sprite = Sprite.FromTexture( Texture.Load( "materials/particle/particle_flares/particle_flare_002.vtex" ) );
		renderer.Additive = true;
		renderer.Scale = 1f;
		renderer.Shadows = false;
		renderer.DepthFeather = 8f;
	}
}
