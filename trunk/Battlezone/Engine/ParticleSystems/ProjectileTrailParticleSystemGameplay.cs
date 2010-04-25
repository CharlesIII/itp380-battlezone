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
    class ProjectileTrailParticleSystemGameplay : ParticleSystem
    {
        public ProjectileTrailParticleSystemGameplay(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 20000;

            settings.Duration = TimeSpan.FromSeconds(1.5);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            settings.MinColor = new Color(255, 255, 255, 128);
            settings.MaxColor = new Color(218, 214, 197, 255);

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 8;
            settings.MaxStartSize = 16;

            settings.MinEndSize = 32;
            settings.MaxEndSize = 64;
        }
    }
}

