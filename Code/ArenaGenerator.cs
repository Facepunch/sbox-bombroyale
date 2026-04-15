using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Facepunch.BombRoyale;

[Title( "Arena Generator" )]
[Category( "Bomb Royale" )]
public class ArenaGenerator : Component, IRestartable
{
	[Property] public List<ArenaTheme> Themes { get; set; } = new();
	[Property] public ModelRenderer GroundRenderer { get; set; }
	[Property, Range( 0f, 1f )] public float FillPercentage { get; set; } = 0.65f;
	[Property] public float PickupSpawnChance { get; set; } = 0.35f;

	[Sync] private string ActiveThemePath { get; set; }

	private ArenaTheme ActiveTheme { get; set; }

	private const float GridSize = 32f;

	private const int InnerExtent = 5;

	private GameObject BreakablesContainer { get; set; }
	private GameObject AmbienceInstance { get; set; }

	void IRestartable.OnRestart()
	{
		Generate();
	}

	protected override void OnStart()
	{
		if ( !string.IsNullOrEmpty( ActiveThemePath ) )
			ApplyTheme( ActiveThemePath );
	}

	private void Generate()
	{
		if ( !Networking.IsHost ) return;

		PickTheme();
		BroadcastTheme( ActiveThemePath );
		SpawnAmbience();
		DestroyBreakables();
		GenerateBreakables();
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void BroadcastTheme( string path )
	{
		ActiveThemePath = path;
		ApplyTheme( path );
	}

	private void ApplyTheme( string path )
	{
		ActiveTheme = ResolveTheme( path );
		ApplyFloorMaterial();
		ApplyBackgroundColor();
		ApplySolidBlockModels();
	}

	private void ApplySolidBlockModels()
	{
		var models = ActiveTheme?.SolidBlockModels;
		if ( models is null || models.Count == 0 ) return;

		var themeSeed = ActiveTheme.ResourcePath?.GetHashCode() ?? 0;
		foreach ( var solid in Scene.GetAllComponents<SolidBlock>() )
		{
			var seed = HashCode.Combine( solid.GameObject.Id, themeSeed );
			var model = models[(int)((uint)seed % models.Count)];

			var renderer = solid.Components.Get<ModelRenderer>();
			if ( renderer.IsValid() )
				renderer.Model = model;

			var collider = solid.Components.Get<ModelCollider>();
			if ( collider.IsValid() )
				collider.Model = model;
		}
	}

	private void SpawnAmbience()
	{
		if ( AmbienceInstance.IsValid() )
		{
			AmbienceInstance.Destroy();
			AmbienceInstance = null;
		}

		var prefab = ActiveTheme?.AmbiencePrefab;
		if ( prefab is null ) return;

		AmbienceInstance = prefab.Clone();
		AmbienceInstance.Name = "Ambience";
		AmbienceInstance.NetworkSpawn();
	}

	private void PickTheme()
	{
		var theme = Themes.Count > 0 ? Game.Random.FromList( Themes ) : null;
		ActiveThemePath = theme?.ResourcePath;
	}

	private ArenaTheme ResolveTheme( string path )
	{
		if ( string.IsNullOrEmpty( path ) ) return null;
		return Themes.FirstOrDefault( t => t is not null && t.ResourcePath == path );
	}

	private void ApplyFloorMaterial()
	{
		if ( !GroundRenderer.IsValid() ) return;
		if ( ActiveTheme?.FloorMaterial is null ) return;

		GroundRenderer.MaterialOverride = ActiveTheme.FloorMaterial;
	}

	private void ApplyBackgroundColor()
	{
		var camera = Scene.Camera;
		if ( !camera.IsValid() ) return;
		if ( ActiveTheme is null ) return;

		camera.BackgroundColor = ActiveTheme.BackgroundColor;
	}

	private void DestroyBreakables()
	{
		if ( BreakablesContainer.IsValid() )
		{
			BreakablesContainer.Destroy();
			BreakablesContainer = null;
		}

		var existing = Scene.GetAllComponents<Bombable>().ToList();
		foreach ( var bombable in existing )
		{
			bombable.GameObject.Destroy();
		}
	}

private void GenerateBreakables()
	{
		BreakablesContainer = new GameObject( true, "Breakables" )
		{
			WorldPosition = Vector3.Zero
		};

		var guaranteed = GetGuaranteedBreakablePositions();
		var validPositions = GetValidBreakablePositions();

		validPositions.RemoveAll( p => guaranteed.Contains( p ) );

		var count = (int)( validPositions.Count * FillPercentage );
		var chosen = validPositions.OrderBy( _ => Game.Random.Next() ).Take( count ).ToList();

		foreach ( var gridPos in guaranteed )
		{
			var worldPos = new Vector3( gridPos.x * GridSize, gridPos.y * GridSize, 0f );
			SpawnBreakable( worldPos, GetRandomBreakableModel() );
		}

		foreach ( var gridPos in chosen )
		{
			var worldPos = new Vector3( gridPos.x * GridSize, gridPos.y * GridSize, 0f );
			SpawnBreakable( worldPos, GetRandomBreakableModel() );
		}

		BreakablesContainer.NetworkSpawn();
	}

	private void SpawnBreakable( Vector3 position, Model model )
	{
		var go = new GameObject( true, "Breakable" )
		{
			Parent = BreakablesContainer,
			WorldPosition = position,
			NetworkMode = NetworkMode.Object
		};

		var renderer = go.Components.Create<ModelRenderer>();
		renderer.Model = model;

		var collider = go.Components.Create<ModelCollider>();
		collider.Model = Model.Load( "models/block_metal_a/block_metal_a.vmdl" );
		collider.Static = false;

		var bombable = go.Components.Create<Bombable>();
		bombable.Renderer = renderer;
		bombable.SpawnPickupChance = PickupSpawnChance;
	}

	private Model GetRandomBreakableModel()
	{
		var models = ActiveTheme?.BreakableBlockModels;
		return models is { Count: > 0 } ? Game.Random.FromList( models ) : Model.Load( "models/block_a.vmdl" );
	}

	private List<Vector2Int> GetValidBreakablePositions()
	{
		var positions = new List<Vector2Int>();
		var spawns = GetSpawnSafeZones();
		var pillars = GetPillarPositions();

		for ( var x = -InnerExtent; x <= InnerExtent; x++ )
		{
			for ( var y = -InnerExtent; y <= InnerExtent; y++ )
			{
				var pos = new Vector2Int( x, y );

				if ( pillars.Contains( pos ) )
					continue;

				if ( spawns.Contains( pos ) )
					continue;

				positions.Add( pos );
			}
		}

		return positions;
	}

	/// <summary>
	/// Interior pillars at every even-even grid position.
	/// </summary>
	private static HashSet<Vector2Int> GetPillarPositions()
	{
		var pillars = new HashSet<Vector2Int>();

		for ( var x = -InnerExtent + 1; x <= InnerExtent - 1; x++ )
		{
			for ( var y = -InnerExtent + 1; y <= InnerExtent - 1; y++ )
			{
				if ( x % 2 == 0 && y % 2 == 0 )
					pillars.Add( new Vector2Int( x, y ) );
			}
		}

		return pillars;
	}

	/// <summary>
	/// Blocks that must always spawn at the edges of each spawn safe zone,
	/// boxing the player into their starting L-shape.
	/// </summary>
	private static HashSet<Vector2Int> GetGuaranteedBreakablePositions()
	{
		var positions = new HashSet<Vector2Int>();

		var spawnCorners = new Vector2Int[]
		{
			new( InnerExtent, -InnerExtent ),
			new( InnerExtent, InnerExtent ),
			new( -InnerExtent, InnerExtent ),
			new( -InnerExtent, -InnerExtent )
		};

		foreach ( var corner in spawnCorners )
		{
			var signX = Math.Sign( corner.x );
			var signY = Math.Sign( corner.y );

			positions.Add( new Vector2Int( corner.x - signX * 2, corner.y ) );
			positions.Add( new Vector2Int( corner.x, corner.y - signY * 2 ) );
		}

		return positions;
	}

	/// <summary>
	/// Each spawn corner needs the spawn tile and the two adjacent tiles kept clear
	/// so the player can always move out.
	/// </summary>
	private static HashSet<Vector2Int> GetSpawnSafeZones()
	{
		var safe = new HashSet<Vector2Int>();

		var spawnCorners = new Vector2Int[]
		{
			new( InnerExtent, -InnerExtent ),
			new( InnerExtent, InnerExtent ),
			new( -InnerExtent, InnerExtent ),
			new( -InnerExtent, -InnerExtent )
		};

		foreach ( var corner in spawnCorners )
		{
			safe.Add( corner );

			var signX = Math.Sign( corner.x );
			var signY = Math.Sign( corner.y );
			safe.Add( new Vector2Int( corner.x - signX, corner.y ) );
			safe.Add( new Vector2Int( corner.x, corner.y - signY ) );
		}

		return safe;
	}
}
