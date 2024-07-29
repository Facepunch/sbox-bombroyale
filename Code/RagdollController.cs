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

	[Broadcast( NetPermission.HostOnly )]
	public void Ragdoll( Vector3 position, Vector3 force )
	{
		IsRagdolled = true;
		Tags.Add( "corpse" );

		if ( !Physics.IsValid() ) return;
		Physics.Enabled = true;
		
		foreach ( var body in Physics.PhysicsGroup.Bodies )
		{
			body.ApplyImpulseAt( position, force * 5000f );
		}
	}

	[Broadcast( NetPermission.HostOnly )]
	public void Unragdoll()
	{
		IsRagdolled = false;
		Tags.Remove( "corpse" );

		if ( !Physics.IsValid() ) return;
		Physics.Enabled = false;
	}

	protected override void OnFixedUpdate()
	{
		Tags.Set( "corpse", IsRagdolled );

		if ( Physics.IsValid() )
		{
			Physics.Enabled = IsRagdolled;
		}
		
		base.OnFixedUpdate();
	}
}
