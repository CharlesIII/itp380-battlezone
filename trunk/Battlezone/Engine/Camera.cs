#region File Description
//------------------------------------------------------------------------------
// Camera.cs
//
// Copyright (C) Double XL, Graham Cracka, Old Jamison Irish Whiskey, & C-Cubed.
// All rights reserved.
//------------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace Battlezone.Engine
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Camera : GameComponent
    {

        #region Chased object properties (set externally each frame)

        /// <summary>
        /// Position of object being chased.
        /// </summary>
        public Vector3 ChasePosition
        {
            get { return chasePosition; }
            set { chasePosition = value; }
        }
        private Vector3 chasePosition;

        /// <summary>
        /// Direction the chased object is facing.
        /// </summary>
        public Vector3 ChaseDirection
        {
            get { return chaseDirection; }
            set { chaseDirection = value; }
        }
        private Vector3 chaseDirection;

        /// <summary>
        /// Chased object's Up vector.
        /// </summary>
        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }
        private Vector3 up = Vector3.UnitY;

        #endregion

        #region Desired camera positioning (set when creating camera or changing view)

        /// <summary>
        /// Desired camera position in the chased object's coordinate system.
        /// </summary>
        public Vector3 DesiredPositionOffset
        {
            get { return desiredPositionOffset; }
            set { desiredPositionOffset = value; }
        }
        private Vector3 desiredPositionOffset = new Vector3(0.0f, 220.0f, 1500.0f);

        /// <summary>
        /// Desired camera position in world space.
        /// </summary>
        public Vector3 DesiredPosition
        {
            get
            {
                // Ensure correct value even if update has not been called this frame
                UpdateWorldPositions();
                return desiredPosition;
            }
        }
        private Vector3 desiredPosition;

        /// <summary>
        /// Look at point in the chased object's coordinate system.
        /// </summary>
        public Vector3 LookAtOffset
        {
            get { return lookAtOffset; }
            set { lookAtOffset = value; }
        }
        private Vector3 lookAtOffset = new Vector3(0.0f, 5.0f, -50.0f);

        /// <summary>
        /// Look at point in world space.
        /// </summary>
        public Vector3 LookAt
        {
            get
            {
                // Ensure correct value even if update has not been called this frame
                UpdateWorldPositions();

                return lookAt;
            }
        }
        private Vector3 lookAt;

        #endregion

        /// <summary>
        /// Constructor for the Camera Class.
        /// </summary>
        /// <param name="game">A reference to the Game.</param>
        public Camera(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            UpdateWorldPositions();

            float cameraColDistance = 0.0f;
            checkCamCollision(out cameraColDistance);

            if (cameraColDistance != 0.0f)
            {
                Vector3 cam2Tank = new Vector3();
                cam2Tank = GameplayScreen.Instance.getPlayer().WorldPosition - desiredPosition;
                float camTankDistance = cam2Tank.Length();
                cam2Tank.Normalize();

                desiredPosition += cam2Tank * (camTankDistance - cameraColDistance);
            }

            //Update Camera
            ChasePosition = GameplayScreen.Instance.getPlayer().WorldPosition;
            Matrix temp = GameplayScreen.Instance.getPlayer().worldTransform * GameplayScreen.Instance.getPlayer().turretBone.Transform;
            ChaseDirection = (temp.Forward * -1);
            Up = Vector3.UnitY;
            GameplayScreen.CameraMatrix = Matrix.CreateLookAt(desiredPosition, LookAt, Up);

           

            base.Update(gameTime);
        }

        /// <summary>
        /// Rebuilds object space values in world space. Invoke before publicly
        /// returning or privately accessing world space values.
        /// </summary>
        private void UpdateWorldPositions()
        {
            // Construct a matrix to transform from object space to worldspace
            Matrix transform = Matrix.Identity;
            transform.Forward = ChaseDirection;
            transform.Up = Up;
            transform.Right = Vector3.Cross(Up, ChaseDirection);

            // Calculate desired camera properties in world space
            desiredPosition = ChasePosition +
                Vector3.TransformNormal(DesiredPositionOffset, transform);
            lookAt = ChasePosition +
                Vector3.TransformNormal(LookAtOffset, transform);
        }

        private bool checkCamCollision(out float distanceToBuilding)
        {
            Vector3 dir;
            Vector3 distance;
            distance = desiredPosition - GameplayScreen.Instance.getPlayer().WorldPosition;
            dir = distance;
            dir.Normalize();

            Ray colCheckRay = new Ray(GameplayScreen.Instance.getPlayer().WorldPosition, dir);
            float? intersection;
            foreach (Actor a in GameplayScreen.Instance.activeActors)
            {
                if (a.COLLISION_IDENTIFIER == 4)
                {
                    BattlezoneObjects.Building b = (BattlezoneObjects.Building)a;
                    intersection = colCheckRay.Intersects(b.WorldBoundsBox);
                    if (intersection != null && ((intersection*intersection) <= distance.LengthSquared()))
                    {
                        distanceToBuilding = (float)intersection;
                        //Console.Out.WriteLine(b.WorldBoundsBox);
                        return true;
                    }
                }
            }
            distanceToBuilding = 0.0f;
            return false;
        }
    }
}