using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.BombRoyale;

public abstract partial class ScreenShake
{
	private static readonly List<ScreenShake> List = new();

	public static void Apply( CameraComponent camera )
	{
		for ( var i = List.Count; i > 0; i-- )
		{
			var entry = List[i - 1];
			var keep = entry.Update( camera );
			if ( keep ) continue;

			entry.OnRemove();
			List.RemoveAt( i - 1 );
		}
	}

	internal static void Add( ScreenShake shake )
	{
		List.Add( shake );
	}

	protected virtual void OnRemove()
	{

	}

	public static void ClearAll()
	{
		List.Clear();
	}

	protected abstract bool Update( CameraComponent camera );
}
