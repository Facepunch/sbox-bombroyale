using Sandbox;

namespace Facepunch.BombRoyale;

/// <summary>
/// Ring sprite particles rising upward on player respawn.
/// Configured via prefabs/effects/respawn.prefab
/// </summary>
public static class RespawnEffect
{
	public static void Create( Scene scene, Vector3 position, Color color )
	{
		var go = GameObject.Clone( "prefabs/effects/respawn.prefab", new CloneConfig
		{
			StartEnabled = true,
			Transform = new Transform( position ),
			Name = "RespawnEffect"
		} );

		if ( !go.IsValid() ) return;

		var effect = go.Components.GetInDescendantsOrSelf<ParticleEffect>();
		if ( effect.IsValid() )
			effect.Tint = color;
	}
}
