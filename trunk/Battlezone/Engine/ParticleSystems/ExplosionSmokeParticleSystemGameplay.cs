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
    class ExplosionSmokeParticleSystemGameplay : ParticleSystem
    {
        public ExplosionSmokeParticleSystemGameplay(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 700;

            settings.Duration = TimeSpan.FromSeconds(4);

            settings.MinHorizontalVelocity = 50;
            settings.MaxHorizontalVelocity = 70;

            settings.MinVerticalVelocity = 130;
            settings.MaxVerticalVelocity = 200;

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
    }
}
