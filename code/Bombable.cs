using Sandbox;
using Editor;

namespace Facepunch.BombRoyale;

[Title( "Bombable" )]
[Category( "Bomb Royale" )]
public class Bombable : Component
{
	[Sync] public bool IsHidden { get; set; }
	
	public bool IsSpaceOccupied()
	{
		if ( !IsHidden ) return true;

		var renderer = Components.Get<ModelRenderer>();
		var bounds = renderer.Bounds + Transform.Position;
		
		var trace = Scene.Trace.Ray( bounds.Center, bounds.Center )
			.Radius( 8f )
			.WithAnyTags( "solid", "pickup", "player", "bomb" )
			.Run();


		if ( trace.Hit )
		{
		    throw new System.Exception("Trace hit an object.");
		}

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

	public void Hide()
	{
		var renderer = Components.Get<ModelRenderer>();
		renderer.Enabled = false;
		IsHidden = true;
	}

	public void Show()
	{
		var renderer = Components.Get<ModelRenderer>();
		renderer.Enabled = true;
		IsHidden = false;
	}
}
