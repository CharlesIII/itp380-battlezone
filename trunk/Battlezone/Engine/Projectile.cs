#region File Description
//-----------------------------------------------------------------------------
// Projectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace Battlezone
{
    /// <summary>
    /// This class demonstrates how to combine several different particle systems
    /// to build up a more sophisticated composite effect. It implements a rocket
    /// projectile, which arcs up into the sky using a ParticleEmitter to leave a
    /// steady stream of trail particles behind it. After a while it explodes,
    /// creating a sudden burst of explosion and smoke particles.
    /// </summary>
    class Projectile
    {
        #region Constants

        const float trailParticlesPerSecond = 200;
        const int numExplosionParticles = 30;
        const int numExplosionSmokeParticles = 50;
        const float sidewaysVelocityRange = 60;
        const float verticalVelocityRange = 40;
        const float gravity = 0;//15;

        #endregion

        #region Fields

       public ParticleSystem explosionParticles;
       public ParticleSystem explosionSmokeParticles;
       public ParticleSystem projectileTrailParticles;
       public ParticleEmitter trailEmitter;

        Vector3 position;
        Vector3 velocity;
        float age;
        float projectileLifespan;

        static Random random = new Random();

        #endregion


        /// <summary>
        /// Constructs a new projectile.
        /// </summary>
        public Projectile(ParticleSystem explosionParticles,
                          ParticleSystem explosionSmokeParticles,
                          ParticleSystem projectileTrailParticles,
                          Vector3 cameraPosition,
                          Vector3 cameraDirection, int screenNum  )
        {
            this.explosionParticles = explosionParticles;
            this.explosionSmokeParticles = explosionSmokeParticles;
            this.projectileTrailParticles = projectileTrailParticles;

            // Start at the origin, firing in a random (but roughly upward) direction.

            position = cameraPosition;

            if (screenNum == 0)
            {
                velocity.X = 0;
                velocity.Y = 0;// cameraDirection.Y * 100.0f; ;//(float)(random.NextDouble() + 0.5) * verticalVelocityRange;
                velocity.Z = 0;// (float)(random.NextDouble() - 0.5) * sidewaysVelocityRange;
                projectileLifespan = 0.0f;
            }
            else if (screenNum == 1)
            {
                velocity.X = cameraDirection.X * 500.0f;
                velocity.Y = 0;// cameraDirection.Y * 100.0f; ;//(float)(random.NextDouble() + 0.5) * verticalVelocityRange;
                velocity.Z = cameraDirection.Z * 500.0f; ;// (float)(random.NextDouble() - 0.5) * sidewaysVelocityRange;
                projectileLifespan = (float)random.Next(2, 5);
            }
            else if (screenNum == 2)
            {
                velocity.X = cameraDirection.X * 500.0f;
                velocity.Y = 0;// cameraDirection.Y * 100.0f; ;//(float)(random.NextDouble() + 0.5) * verticalVelocityRange;
                velocity.Z = cameraDirection.Z * 500.0f; ;// (float)(random.NextDouble() - 0.5) * sidewaysVelocityRange;
                projectileLifespan = 10;
            }

            // Use the particle emitter helper to output our trail particles.
            trailEmitter = new ParticleEmitter(projectileTrailParticles,
                                               trailParticlesPerSecond, position);
        }


        /// <summary>
        /// Updates the projectile.
        /// </summary>
        public bool Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Simple projectile physics.
            position += velocity * elapsedTime;
            velocity.Y -= elapsedTime * gravity;
            age += elapsedTime;

            // Update the particle emitter, which will create our particle trail.
            trailEmitter.Update(gameTime, position);

            // If enough time has passed, explode! Note how we pass our velocity
            // in to the AddParticle method: this lets the explosion be influenced
            // by the speed and direction of the projectile which created it.
            if (age > projectileLifespan)
            {
                for (int i = 0; i < numExplosionParticles; i++)
                    explosionParticles.AddParticle(position, velocity);

                for (int i = 0; i < numExplosionSmokeParticles; i++)
                    explosionSmokeParticles.AddParticle(position, velocity);

                return false;
            }
                
            return true;
        }
    }
}
