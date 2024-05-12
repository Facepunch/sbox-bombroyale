using Sandbox;
using System;
using System.Linq;

namespace Facepunch.BombRoyale;

[Title( "Ragdoll Controller" )]
[Group( "Bomb Royale" )]
public sealed class RagdollController : Component
{
	[HostSync] public bool IsRagdolled { get; private set; }
	[Property] public ModelPhysics Physics { get; set; }

	[Broadcast]
	public void Ragdoll( Vector3 position, Vector3 force )
	{
		IsRagdolled = true;
		Physics.Enabled = true;
		Tags.Add( "corpse" );
		
		foreach ( var body in Physics.PhysicsGroup.Bodies )
		{
			body.ApplyImpulseAt( position, force * 5000f );
		}
	}

	[Broadcast]
	public void Unragdoll()
	{
		IsRagdolled = false;
		Physics.Enabled = false;
		Tags.Remove( "corpse" );
	}

	protected override void OnStart()
	{
		if ( IsRagdolled )
		{
			Physics.Enabled = true;
			Tags.Add( "corpse" );
		}
		
		base.OnStart();
	}
}
