using Sandbox;
using Editor;

namespace Facepunch.BombRoyale
{
	[AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
	[Description( "Maps can have multiple arenas. An arena will be picked at random each time a game is played." )]
	[Title( "Arena" )]
	[Solid, HammerEntity]
	public partial class Arena : ModelEntity
	{
		[Description( "A unique id for this arena. Player spawnpoints for this arena should use the same id." )]
		[Property] public int ArenaId { get; set; }

		public override void Spawn()
		{
			EnableAllCollisions = false;
			Transmit = TransmitType.Always;
			Tags.Add( "trigger" );
			base.Spawn();
		}
	}
}
