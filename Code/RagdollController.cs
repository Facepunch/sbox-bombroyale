using Sandbox;
using System;
using System.Linq;
using Sandbox.Diagnostics;

namespace Facepunch.BombRoyale;

[Title( "Ragdoll Controller" )]
[Group( "Bomb Royale" )]
public sealed class RagdollController : Component
{
	[Sync( SyncFlags.FromHost )] public bool IsRagdolled { get; private set; }
	[Property] public ModelPhysics Physics { get; set; }

	[Rpc.Broadcast( NetFlags.HostOnly )]
	public void Ragdoll( Vector3 position, Vector3 force )
	{
		Physics.Enabled = true;
		IsRagdolled = true;
		Tags.Add( "corpse" );
		
		foreach ( var body in Physics.PhysicsGroup.Bodies )
		{
			body.ApplyImpulseAt( position, force * 5000f );
		}
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	public void Unragdoll()
	{
		Physics.Enabled = false;
		IsRagdolled = false;
		Tags.Remove( "corpse" );
	}
}
