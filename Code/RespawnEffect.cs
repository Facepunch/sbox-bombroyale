using Sandbox;

namespace Facepunch.BombRoyale;

/// <summary>
/// Ring sprite particles rising upward on player respawn.
/// Replaces particles/gameplay/player/respawn/respawn_effect.vpcf
/// </summary>
[Title( "Respawn Effect" )]
[Category( "Bomb Royale" )]
public class RespawnEffect : Component
{
	public static void Create( Scene scene, Vector3 position, Color color )
	{
		var go = new GameObject( false, "RespawnEffect" )
		{
			WorldPosition = position
		};

		var effect = go.AddComponent<RespawnEffect>();
		effect.EffectColor = color;

		go.AddComponent<TemporaryEffect>().DestroyAfterSeconds = 3f;
		go.Enabled = true;
	}

	private Color EffectColor { get; set; } = Color.White;

	protected override void OnEnabled()
	{
		var effect = Components.GetOrCreate<ParticleEffect>();
		effect.MaxParticles = 20;
		effect.Lifetime = 0.8f;
		effect.ApplyColor = true;
		effect.Tint = EffectColor;
		effect.ApplyAlpha = true;
		effect.Alpha = new ParticleFloat { Type = ParticleFloat.ValueType.Range, Evaluation = ParticleFloat.EvaluationType.Life, ConstantA = 1f, ConstantB = 0f };
		effect.ApplyShape = true;
		effect.Scale = new ParticleFloat { Type = ParticleFloat.ValueType.Range, Evaluation = ParticleFloat.EvaluationType.Life, ConstantA = 3f, ConstantB = 1f };
		effect.Force = true;
		effect.ForceDirection = new Vector3( 0, 0, 200f );

		var emitter = Components.GetOrCreate<ParticleSphereEmitter>();
		emitter.Radius = 10f;
		emitter.Velocity = 0f;
		emitter.Loop = false;
		emitter.Duration = 1f;
		emitter.Burst = 0;
		emitter.Rate = 20f;

		var renderer = Components.GetOrCreate<ParticleSpriteRenderer>();
		renderer.Sprite = Sprite.FromTexture( Texture.Load( "materials/particles/particle_ring_soft_inner.vtex" ) );
		renderer.Additive = true;
		renderer.Scale = 3f;
		renderer.Shadows = false;
		renderer.FogStrength = 1f;
		renderer.DepthFeather = 8f;
	}
}
