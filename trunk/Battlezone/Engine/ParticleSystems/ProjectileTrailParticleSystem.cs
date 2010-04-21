#region File Description
//-----------------------------------------------------------------------------
// ProjectileTrailParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Battlezone
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class ProjectileTrailParticleSystem : ParticleSystem
    {
        public ProjectileTrailParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 2000;

            settings.Duration = TimeSpan.FromSeconds(5);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            settings.MinColor = new Color(64, 96, 128, 255);
            settings.MaxColor = new Color(255, 255, 255, 128);

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 6;
            settings.MaxStartSize = 8;

            settings.MinEndSize = 9;
            settings.MaxEndSize = 19;
        }

        public void InitializeSettings(int MaxParticles, double time, float DurationRandomness,
                                            int MinHorizontalVelocity, int MaxHorizontalVelocity, int MinVerticalVelocity,
                                            int MaxVerticalVelocity, int MinStartSize, int MaxStartSize, int MinEndSize, int MaxEndSize)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = MaxParticles;

            settings.Duration = TimeSpan.FromSeconds(time);

            settings.DurationRandomness = DurationRandomness;

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.MinHorizontalVelocity = MinHorizontalVelocity;
            settings.MaxHorizontalVelocity = MaxHorizontalVelocity;

            settings.MinVerticalVelocity = MinVerticalVelocity;
            settings.MaxVerticalVelocity = MaxVerticalVelocity;

            settings.MinColor = new Color(64, 96, 128, 255);
            settings.MaxColor = new Color(255, 255, 255, 128);

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = MinStartSize;
            settings.MaxStartSize = MaxStartSize;

            settings.MinEndSize = MinEndSize;
            settings.MaxEndSize = MaxEndSize;
        }
    }
}
