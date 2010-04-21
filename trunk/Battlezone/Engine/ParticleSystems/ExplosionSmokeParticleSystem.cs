#region File Description
//-----------------------------------------------------------------------------
// ExplosionSmokeParticleSystem.cs
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
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class ExplosionSmokeParticleSystem : ParticleSystem
    {
        public ExplosionSmokeParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 700;

            settings.Duration = TimeSpan.FromSeconds(4);

            settings.MinHorizontalVelocity = 50;
            settings.MaxHorizontalVelocity = 70;

            settings.MinVerticalVelocity = 200;
            settings.MaxVerticalVelocity = 230;

            settings.Gravity = new Vector3(0, -10, 0);

            settings.EndVelocity = 0;

            settings.MinColor = Color.LightGray;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 100;
            settings.MaxEndSize = 200;
        }

        public void InitializeSettings(int MaxParticles, int time, int MinHorizontalVelocity,
                                       int MaxHorizontalVelocity, int MinVerticalVelocity, 
                                       int MaxVerticalVelocity, Vector3 Gravity, int MinStartSize, 
                                       int MaxStartSize, int MinEndSize, int MaxEndSize)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = MaxParticles;

            settings.Duration = TimeSpan.FromSeconds(time);

            settings.MinHorizontalVelocity = MinHorizontalVelocity;
            settings.MaxHorizontalVelocity = MaxHorizontalVelocity;

            settings.MinVerticalVelocity = MinVerticalVelocity;
            settings.MaxVerticalVelocity = MaxVerticalVelocity;

            settings.Gravity = Gravity;

            settings.EndVelocity = 0;

            settings.MinColor = Color.LightGray;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = MinStartSize;
            settings.MaxStartSize = MaxStartSize;

            settings.MinEndSize = MinEndSize;
            settings.MaxEndSize = MaxEndSize;
        }
    }
}
