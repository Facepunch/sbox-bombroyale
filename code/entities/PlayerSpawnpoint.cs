using Sandbox;
using Editor;

namespace Facepunch.BombRoyale
{
	[EditorModel( "models/editor/playerstart.vmdl", FixedBounds = true )]
	[Title( "Player Spawnpoint" )]
	[HammerEntity]
	public partial class PlayerSpawnpoint : Entity
	{
		[Description( "Set this to the same id as the arena it belongs to." )]
		[Property] public int ArenaId { get; set; }
	}
}
