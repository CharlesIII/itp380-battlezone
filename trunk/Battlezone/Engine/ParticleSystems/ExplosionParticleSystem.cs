#region File Description
//-----------------------------------------------------------------------------
// ExplosionParticleSystem.cs
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
    /// Custom particle system for creating the fiery part of the explosions.
    /// </summary>
    class ExplosionParticleSystem : ParticleSystem
    {
        public ExplosionParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "explosion";

            settings.MaxParticles = 400;

            settings.Duration = TimeSpan.FromSeconds(2);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 20;
            settings.MaxHorizontalVelocity = 100;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 200;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 100;
            settings.MaxStartSize = 100;

            settings.MinEndSize = 100;
            settings.MaxEndSize = 200;

            // Use additive blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;
        }

        public void InitializeSettings(int MaxParticles, int time, int DurationRandomness,
                                                    int MinHorizontalVelocity, int MaxHorizontalVelocity, int MinVerticalVelocity,
                                                    int MaxVerticalVelocity, int MinStartSize, int MaxStartSize, int MinEndSize, int MaxEndSize)
        {
            settings.TextureName = "explosion";

            settings.MaxParticles = MaxParticles;

            settings.Duration = TimeSpan.FromSeconds(time);
            settings.DurationRandomness = DurationRandomness;

            settings.MinHorizontalVelocity = MinHorizontalVelocity;
            settings.MaxHorizontalVelocity = MaxHorizontalVelocity;

            settings.MinVerticalVelocity = MinVerticalVelocity;
            settings.MaxVerticalVelocity = MaxVerticalVelocity;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = MinStartSize;
            settings.MaxStartSize = MaxStartSize;

            settings.MinEndSize = MinEndSize;
            settings.MaxEndSize = MaxEndSize;

            // Use additive blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;
        }
    }
}
