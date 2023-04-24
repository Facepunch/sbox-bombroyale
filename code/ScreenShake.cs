using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.BombRoyale;

public abstract partial class ScreenShake
{
	internal static List<ScreenShake> List = new();

	internal static void Apply()
	{
		for ( int i = List.Count; i > 0; i-- )
		{
			var entry = List[i - 1];
			var keep = entry.Update();

			if ( !keep )
			{
				entry.OnRemove();
				List.RemoveAt( i - 1 );
			}
		}
	}

	internal static void Add( ScreenShake shake )
	{
		if ( Prediction.FirstTime )
		{
			List.Add( shake );
		}
	}

	protected virtual void OnRemove()
	{

	}

	public static void ClearAll()
	{
		List.Clear();
	}

	public abstract bool Update();
}
