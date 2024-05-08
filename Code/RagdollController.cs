using Sandbox;
using System;
using System.Linq;

namespace Facepunch.BombRoyale;

[Title( "Ragdoll Controller" )]
[Group( "Bomb Royale" )]
public sealed class RagdollController : Component
{
	public bool IsRagdolled => RagdollObject.IsValid();
	
	private GameObject RagdollObject { get; set; }

	[Broadcast]
	public void Ragdoll( Vector3 position, Vector3 force )
	{
		
	}

	[Broadcast]
	public void Unragdoll()
	{
		
	}
}
