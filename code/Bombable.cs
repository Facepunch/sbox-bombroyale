using Sandbox;
using Editor;
using Sandbox.Diagnostics;

namespace Facepunch.BombRoyale;

[Title( "Bombable" )]
[Category( "Bomb Royale" )]
public class Bombable : Component, IRestartable
{
	[Sync] public bool IsHidden { get; set; }
	
	[Property] public ModelRenderer Renderer { get; set; }
	
	void IRestartable.OnRestart()
	{
		Show();
	}
	
	public bool IsSpaceOccupied()
	{
		if ( !IsHidden ) return true;

		var bounds = Renderer.Bounds;
		
		var trace = Scene.Trace.Ray( bounds.Center, bounds.Center )
			.Radius( 8f )
			.WithAnyTags( "solid", "pickup", "player", "bomb" )
			.Run();

		return trace.Hit;
	}

	public void TrySpawnPickup()
	{
		/*
		if ( Game.Random.Float() < 0.35f )
		{
			var p = Pickup.CreateRandom();
			p.Position = WorldSpaceBounds.Center;
			return;
		}
		*/
	}

	[Broadcast( NetPermission.HostOnly )]
	public void Hide()
	{
		Assert.True( Networking.IsHost );
		Renderer.Enabled = false;
		Tags.Add( "passplayers" );
		IsHidden = true;
	}
	
	protected override void OnAwake()
	{
		Tags.Add( "solid" );
		base.OnAwake();
	}
	
	[Broadcast( NetPermission.HostOnly )]
	private void Show()
	{
		Renderer.Enabled = true;
		Tags.Remove( "passplayers" );
		IsHidden = false;
	}
}
