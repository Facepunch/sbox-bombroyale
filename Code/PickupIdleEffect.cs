using Sandbox;

namespace Facepunch.BombRoyale;

/// <summary>
/// Continuous glowing flare particles around a pickup.
/// Replaces particles/gameplay/idle_coin/idle_coin.vpcf
/// </summary>
[Title( "Pickup Idle Effect" )]
[Category( "Bomb Royale" )]
public class PickupIdleEffect : Component
{
	public Color EffectColor { get; set; } = Color.Orange;

	protected override void OnEnabled()
	{
		var effect = Components.GetOrCreate<ParticleEffect>();
		effect.MaxParticles = 10;
		effect.Lifetime = new ParticleFloat( 0.25f, 0.5f );
		effect.ApplyColor = true;
		effect.Tint = EffectColor;
		effect.ApplyAlpha = true;
		effect.Alpha = new ParticleFloat { Type = ParticleFloat.ValueType.Range, Evaluation = ParticleFloat.EvaluationType.Life, ConstantA = 1f, ConstantB = 0f };
		effect.ApplyShape = true;
		effect.Scale = new ParticleFloat { Type = ParticleFloat.ValueType.Range, Evaluation = ParticleFloat.EvaluationType.Life, ConstantA = 2f, ConstantB = 0f };

		var emitter = Components.GetOrCreate<ParticleSphereEmitter>();
		emitter.Radius = 20f;
		emitter.Velocity = 0f;
		emitter.OnEdge = true;
		emitter.Loop = true;
		emitter.Duration = 10f;
		emitter.Burst = 0;
		emitter.Rate = 1f;

		var renderer = Components.GetOrCreate<ParticleSpriteRenderer>();
		renderer.Sprite = Sprite.FromTexture( Texture.Load( "materials/particle/particle_flares/particle_flare_002.vtex" ) );
		renderer.Additive = true;
		renderer.Scale = 2f;
		renderer.Shadows = false;
	}
}
