using System.Linq;
using Sandbox;
using Editor;

namespace Facepunch.BombRoyale;

[Group( "Bomb Royale" )]
[Title( "Post Processing" )]
public sealed class PostProcessing : Component
{
	private Pixelate Pixelate { get; set; }
	private ColorAdjustments ColorAdjustments { get; set; }
	private Sharpen Sharpen { get; set; }
	private ChromaticAberration ChromaticAberration { get; set; }
	
	protected override void OnStart()
	{
		Pixelate = Components.Get<Pixelate>();
		ColorAdjustments = Components.Get<ColorAdjustments>();
		Sharpen = Components.Get<Sharpen>();
		ChromaticAberration = Components.Get<ChromaticAberration>();
		
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		var sum = ScreenShake.List.OfType<ScreenShake.Random>().Sum( s => (1f - s.Progress) );

		Pixelate.Scale = 0.02f * sum;
		ColorAdjustments.Saturation = 1.1f;

		if ( StateSystem.Active is SummaryState )
		{
			ColorAdjustments.Saturation = 0.25f;
		}

		ColorAdjustments.Contrast = 1f;

		var me = Player.Me;

		if ( me.IsValid() && me.LastTakeDamageTime.Absolute > 0f && me.LastTakeDamageTime < 1f )
		{
			var delta = 1f - ((1f / 1f) * me.LastTakeDamageTime);
			Pixelate.Scale += (0.05f * delta);
			ColorAdjustments.Saturation -= (0.5f * delta);
			ColorAdjustments.Contrast += (0.1f * delta);
		}

		ChromaticAberration.Scale = 0.03f + (0.05f * sum);
		Sharpen.Scale = 0.1f;
		
		base.OnUpdate();
	}
}
