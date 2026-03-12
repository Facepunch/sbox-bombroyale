using Sandbox;

namespace Facepunch.BombRoyale;

/// <summary>
/// Spawns debris model particles bursting outward when a block is destroyed.
/// Configured via prefabs/effects/block_break.prefab
/// </summary>
public static class BlockBreakEffect
{
	public static void Create( Scene scene, Vector3 position )
	{
		GameObject.Clone( "prefabs/effects/block_break.prefab", new CloneConfig
		{
			Transform = new Transform( position ),
			Name = "BlockBreak"
		} );
	}
}
