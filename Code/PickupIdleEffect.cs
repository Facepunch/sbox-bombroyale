using Sandbox;

namespace Facepunch.BombRoyale;

/// <summary>
/// Continuous glowing flare particles around a pickup.
/// Configured via prefabs/effects/pickup_idle.prefab
/// </summary>
public static class PickupIdleEffect
{
	public static GameObject Create( GameObject parent, Color color )
	{
		var go = GameObject.Clone( "prefabs/effects/pickup_idle.prefab", new CloneConfig
		{
			Transform = Transform.Zero,
			Parent = parent,
			Name = "PickupIdleEffect"
		} );

		if ( !go.IsValid() ) return go;

		var effect = go.Components.Get<ParticleEffect>();
		if ( effect.IsValid() )
			effect.Tint = color;

		return go;
	}
}
