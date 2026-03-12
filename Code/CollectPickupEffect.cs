using Sandbox;

namespace Facepunch.BombRoyale;

/// <summary>
/// Burst of particles when a pickup is collected.
/// Configured via prefabs/effects/collect_pickup.prefab
/// </summary>
public static class CollectPickupEffect
{
	public static void Create( Scene scene, Vector3 position, Color color )
	{
		var go = GameObject.Clone( "prefabs/effects/collect_pickup.prefab", new CloneConfig
		{
			StartEnabled = true,
			Transform = new Transform( position ),
			Name = "CollectPickup"
		} );

		if ( !go.IsValid() ) return;

		var effect = go.Components.Get<ParticleEffect>();
		if ( effect.IsValid() )
			effect.Tint = color;
	}
}
