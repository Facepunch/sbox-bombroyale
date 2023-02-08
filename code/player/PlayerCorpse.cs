using Sandbox;

namespace Facepunch.BombRoyale
{
	public class PlayerCorpse : ModelEntity
	{
		private TimeSince TimeSinceSpawned { get; set; }

		public PlayerCorpse()
		{
			UsePhysicsCollision = true;
			PhysicsEnabled = true;

			Tags.Add( "corpse" );
		}

		public void CopyFrom( BombRoyalePlayer player )
		{
			SetModel( player.GetModelName() );
			TakeDecalsFrom( player );

			// We have to use `this` to refer to the extension methods.
			this.CopyBonesFrom( player );
			this.SetRagdollVelocityFrom( player );

			foreach ( var child in player.Children )
			{
				if ( child is ModelEntity e )
				{
					var model = e.GetModelName();

					if ( model != null && !model.Contains( "clothes" ) )
						continue;

					var clothing = new ModelEntity();
					clothing.SetModel( model );
					clothing.SetParent( this, true );
				}
			}
		}

		public void ApplyForceToBone( Vector3 force, int forceBone )
		{
			PhysicsGroup.AddVelocity( force );

			if ( forceBone >= 0 )
			{
				var body = GetBonePhysicsBody( forceBone );

				if ( body != null )
				{
					body.ApplyForce( force * 1000 );
				}
				else
				{
					PhysicsGroup.AddVelocity( force );
				}
			}
		}

		public override void Spawn()
		{
			TimeSinceSpawned = 0f;
			base.Spawn();
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( TimeSinceSpawned > 5f )
			{
				Delete();
				return;
			}

			var opacity = 1f - ((1f / 10f) * TimeSinceSpawned);

			RenderColor = RenderColor.WithAlpha( opacity );

			foreach ( var child in Children )
			{
				if ( child is ModelEntity m )
				{
					m.RenderColor = m.RenderColor.WithAlpha( opacity );
				}
			}
		}
	}
}
