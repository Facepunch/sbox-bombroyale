using System;
using Sandbox;
using Sandbox.ModelEditor.Nodes;

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
		if ( Game.Random.Float() < 0.35f )
		{
			Pickup.CreateRandom( Renderer.Bounds.Center );
		}
	}

	[Broadcast( NetPermission.HostOnly )]
	public void Break()
	{
		var rb = Components.Get<Rigidbody>();
		var breaklist = Renderer.Model.GetData<ModelBreakPiece[]>();

		if ( breaklist == null || breaklist.Length <= 0 )
			return;

		foreach ( var model in breaklist )
		{
			var gib = new GameObject( true, $"{GameObject.Name} (gib)" );

			gib.Transform.Position = Transform.World.PointToWorld( model.Offset );
			gib.Transform.Rotation = Transform.Rotation;
			gib.Transform.Scale = Transform.Scale;

			foreach ( var tag in model.CollisionTags.Split( ' ', StringSplitOptions.RemoveEmptyEntries ) )
			{
				gib.Tags.Add( tag );
			}

			var c = gib.Components.Create<Gib>( false );
			c.FadeTime = model.FadeTime;
			c.Model = Model.Load( model.Model );
			c.Enabled = true;

			var phys = gib.Components.Get<Rigidbody>( true );

			if ( phys is not null && rb is not null )
			{
				phys.Velocity = rb.Velocity;
				phys.AngularVelocity = rb.AngularVelocity;
			}
		}
	}

	[Broadcast( NetPermission.HostOnly )]
	public void Hide()
	{
		Renderer.Enabled = false;
		Tags.Add( "passable" );
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
		Tags.Remove( "passable" );
		IsHidden = false;
	}
}
