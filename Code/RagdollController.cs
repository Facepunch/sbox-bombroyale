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
		
		var group = Physics.PhysicsGroup;
		if ( !group.IsValid() ) return;
		
		foreach ( var body in Physics.PhysicsGroup.Bodies )
		{
			if ( !body.IsValid() ) continue;
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

	protected override void OnStart()
	{
		if ( IsRagdolled )
		{
			if ( Physics.IsValid() )
				Physics.Enabled = true;
			
			Tags.Add( "corpse" );
		}
		
		base.OnStart();
	}
}
