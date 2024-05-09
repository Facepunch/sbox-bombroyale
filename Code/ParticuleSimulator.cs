using Sandbox;
using Editor;

namespace Facepunch.BombRoyale;

[Hide]
public sealed class ParticuleSimulator : Component
{
	private SceneParticles Particles { get; set; }
	
	public void Simulate( SceneParticles particles )
	{
		Particles = particles;
	}
	
	protected override void OnUpdate()
	{
		if ( !Particles.IsValid() || Particles.Finished )
		{
			GameObject.Destroy();
			return;
		}

		Particles.Simulate( Time.Delta );
		
		base.OnUpdate();
	}

	protected override void OnDestroy()
	{
		Particles?.Delete();
		Particles = null;
		
		base.OnDestroy();
	}
}
