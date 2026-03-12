using Sandbox;

namespace Facepunch.BombRoyale;

[Title( "Bombable" )]
[Category( "Bomb Royale" )]
public class Bombable : Component, IRestartable
{
	[Sync] public bool IsHidden { get; set; }
	
	[Property] public ModelRenderer Renderer { get; set; }
	[Property] public float SpawnPickupChance { get; set; } = 0.35f;
	
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
			.HitTriggers()
			.Run();

		return trace.Hit;
	}

	public void TrySpawnPickup()
	{
		if ( Game.Random.Float() < SpawnPickupChance )
		{
			Pickup.CreateRandom( Renderer.Bounds.Center );
		}
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	public void Break()
	{
		BlockBreakEffect.Create( Scene, Renderer.Bounds.Center );
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	public void Hide()
	{
		Renderer.Enabled = false;
		Tags.Add( "destroyed" );
		Tags.Add( "passable" );
		IsHidden = true;
	}
	
	protected override void OnAwake()
	{
		Tags.Add( "solid" );
		base.OnAwake();
	}
	
	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void Show()
	{
		Renderer.Enabled = true;
		Tags.Remove( "destroyed" );
		Tags.Remove( "passable" );
		IsHidden = false;
	}
}
