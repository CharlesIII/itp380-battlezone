#region File Description
//-----------------------------------------------------------------------------
// SmokePlumeParticleSystem.cs
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
    /// Custom particle system for creating a giant plume of long lasting smoke.
    /// </summary>
    class TankCannonPlumeParticleSystem : ParticleSystem
    {
        public TankCannonPlumeParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 3000;

            settings.Duration = TimeSpan.FromSeconds(3);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 10;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 20;

            // Create a wind effect by tilting the gravity vector sideways.
            settings.Gravity = new Vector3(0, 20, 0);

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 50;
            settings.MaxStartSize = 60;

            settings.MinEndSize = 90;
            settings.MaxEndSize = 400;
        }

        public void setGravity(Vector3 grav)
        {
            settings.Gravity = grav;
        }
    }
}

