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
    class ExplosionSmokeParticleSystemTank : ParticleSystem
    {
        public ExplosionSmokeParticleSystemTank(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 1600;

            settings.Duration = TimeSpan.FromSeconds(10);

            settings.MinHorizontalVelocity = 25;
            settings.MaxHorizontalVelocity = 150;

            settings.MinVerticalVelocity = 100;
            settings.MaxVerticalVelocity = 300;

            settings.Gravity = new Vector3(0, 0, 0);

            settings.EndVelocity = 0;

            settings.MinColor = Color.LightGray;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = 50;
            settings.MaxStartSize = 90;

            settings.MinEndSize = 400;
            settings.MaxEndSize = 400;
        }
    }
}