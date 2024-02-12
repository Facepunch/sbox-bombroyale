using System.Linq;
using Sandbox;

namespace Facepunch.BombRoyale;

[Title( "Bomb" )]
[Category( "Bomb Royale" )]
public class Bomb : Component
{
	public Player Player =>
		Scene.GetAllComponents<Player>().FirstOrDefault( p => p.Network.OwnerId == Network.OwnerId );
	
	[Sync] public bool IsPlaced { get; private set; }
	[Sync] private TimeSince TimeSincePlaced { get; set; }
	[Sync] public int Range { get; private set; }

	private TimeUntil NextBlinkTime { get; set; }
	private TimeUntil BlinkEndTime { get; set; }
	private float LifeTime { get; set; } = 4f;
	private SoundHandle FuseSound { get; set; }
	private bool HasExploded { get; set; }
}
