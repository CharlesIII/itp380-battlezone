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
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Battlezone.Engine;
using Battlezone.BattlezoneObjects;
using System.Timers;
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
    class Projectile : Actor
    {
        #region Constants

        float trailParticlesPerSecond = 200;
        int numExplosionParticles = 30;
        int numExplosionSmokeParticles = 50;
        float sidewaysVelocityRange = 60;
        float verticalVelocityRange = 40;
        float gravity = 0;//15;

        #endregion

        #region Fields

       public ParticleSystem explosionParticles;
       public ParticleSystem explosionSmokeParticles;
       public ParticleSystem projectileTrailParticles;
       public ParticleEmitter trailEmitter;

        Vector3 position;
        Vector3 velocity;
        Vector3 fireDirection;
        float age;
        float projectileLifespan;


        static Random random = new Random();

        private System.Timers.Timer explodeTimer;

        private bool justMade = true;

        #endregion


        /// <summary>
        /// Constructs a new projectile.
        /// </summary>
        public Projectile(ParticleSystem explosionParticles,
                          ParticleSystem explosionSmokeParticles,
                          ParticleSystem projectileTrailParticles,
                          Vector3 cameraPosition,
                          Vector3 cameraDirection, int screenNum, Game game  ) : base(game)
        {


            this.explosionParticles = explosionParticles;
            this.explosionSmokeParticles = explosionSmokeParticles;
            this.projectileTrailParticles = projectileTrailParticles;

            sMeshToLoad = "Missile";

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
                projectileLifespan = (float)random.Next(1, 4);
            }
            else if (screenNum == 2)
            {
                velocity.X = cameraDirection.X * 500.0f;
                velocity.Y = 0;// cameraDirection.Y * 100.0f; ;//(float)(random.NextDouble() + 0.5) * verticalVelocityRange;
                velocity.Z = cameraDirection.Z * 500.0f; ;// (float)(random.NextDouble() - 0.5) * sidewaysVelocityRange;
                projectileLifespan = 1.0f;
            }

            // Use the particle emitter helper to output our trail particles.
            trailEmitter = new ParticleEmitter(projectileTrailParticles,
                                               trailParticlesPerSecond, position);
        }

        public Projectile(ContentManager content, Vector3 Position, Vector3 Direction, Game Game)
            : base(Game)
        {

            ParticleSystem explosionParticles = new ExplosionParticleSystem(Game, content);
            ParticleSystem explosionSmokeParticles = new ExplosionSmokeParticleSystemGameplay(Game, content);
            ParticleSystem projectileTrailParticles = new ProjectileTrailParticleSystemGameplay(Game, content);

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;


            this.explosionParticles = explosionParticles;
            this.explosionSmokeParticles = explosionSmokeParticles;
            this.projectileTrailParticles = projectileTrailParticles;

            // Register the particle system components.
            Game.Components.Add(this.explosionParticles);
            Game.Components.Add(this.explosionSmokeParticles);
            Game.Components.Add(this.projectileTrailParticles);

            sMeshToLoad = "Missile";

            // Start at the origin, firing in a random (but roughly upward) direction.

            position = Position;


            velocity.X = Direction.X * 500.0f;
            velocity.Y = 0;// cameraDirection.Y * 100.0f; ;//(float)(random.NextDouble() + 0.5) * verticalVelocityRange;
            velocity.Z = Direction.Z * 500.0f; ;// (float)(random.NextDouble() - 0.5) * sidewaysVelocityRange;
            projectileLifespan = 1;

            // Use the particle emitter helper to output our trail particles.
            trailEmitter = new ParticleEmitter(projectileTrailParticles,
                                               trailParticlesPerSecond, position);

            explodeTimer = new System.Timers.Timer(3000);
            explodeTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            fireDirection = Direction;

        }

        public void Initialize(float trailParticlesPerSecond, int numExplosionParticles, int numExplosionSmokeParticles,
                               float sidewaysVelocityRange, float verticalVelocityRange, float gravity)
        {
            this.trailParticlesPerSecond = trailParticlesPerSecond;
            this.numExplosionParticles = numExplosionParticles;
            this.numExplosionSmokeParticles = numExplosionSmokeParticles;
            this.sidewaysVelocityRange = sidewaysVelocityRange;
            this.verticalVelocityRange = verticalVelocityRange;
            this.gravity = gravity;//15;
        }

        public override void Initialize()
        {
            base.Initialize();

            Quat *= Quaternion.CreateFromYawPitchRoll(fireDirection.X, fireDirection.Y, fireDirection.Z);
        }

        /// <summary>
        /// Updates the projectile.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!dead)
            {

                // Simple projectile physics.
                position += velocity * elapsedTime;
                velocity.Y -= elapsedTime * gravity;
                age += elapsedTime;

                // Update the particle emitter, which will create our particle trail.
                trailEmitter.Update(gameTime, position);
              
                // If enough time has passed, explode! Note how we pass our velocity
                // in to the AddParticle method: this lets the explosion be influenced
                // by the speed and direction of the projectile which created it.
                if (age > projectileLifespan && !dead)
                {
                    Explode();
                    dead = true;
                }

                explosionParticles.SetCamera(GameplayScreen.CameraMatrix, GameplayScreen.ProjectionMatrix);
                explosionSmokeParticles.SetCamera(GameplayScreen.CameraMatrix, GameplayScreen.ProjectionMatrix);
                projectileTrailParticles.SetCamera(GameplayScreen.CameraMatrix, GameplayScreen.ProjectionMatrix);
            }
                
        }

        public void Explode()
        {
            for (int i = 0; i < numExplosionParticles; i++)
                explosionParticles.AddParticle(position, velocity);

            for (int i = 0; i < numExplosionSmokeParticles; i++)
                explosionSmokeParticles.AddParticle(position, velocity);

            explodeTimer.Start();

        }

        public override void removeSelf()
        {
            Game.Components.Remove(this.explosionParticles);
            Game.Components.Remove(this.explosionSmokeParticles);
            Game.Components.Remove(this.projectileTrailParticles);
            base.removeSelf();
        }

        public bool Update(GameTime gameTime, int type)
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

        public void OnTimedEvent(object sender, EventArgs eArgs)
        {
            removeSelf();
        }
    }
}
