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

namespace Battlezone.BattlezoneObjects
{
    /// <summary>
    /// Inherits from Actor. This is the tank controlled by the player. Should contain everything
    /// needed by a player.
    /// </summary>
    public class PlayerTank : Actor
    {

        #region Bone Fields


        // The XNA framework Model object that we are going to display.
        Model tankModel;


        // Shortcut references to the bones that we are going to animate.
        // We could just look these up inside the Draw method, but it is more
        // efficient to do the lookups while loading and cache the results.
        ModelBone leftBackWheelBone;
        ModelBone rightBackWheelBone;
        ModelBone leftFrontWheelBone;
        ModelBone rightFrontWheelBone;
        ModelBone leftSteerBone;
        ModelBone rightSteerBone;
        public ModelBone turretBone;
        ModelBone cannonBone;
        ModelBone hatchBone;


        // Store the original transform matrix for each animating bone.
        Matrix leftBackWheelTransform;
        Matrix rightBackWheelTransform;
        Matrix leftFrontWheelTransform;
        Matrix rightFrontWheelTransform;
        Matrix leftSteerTransform;
        Matrix rightSteerTransform;
        public Matrix turretTransform;
        Matrix cannonTransform;
        Matrix hatchTransform;


        // Array holding all the bone transform matrices for the entire model.
        // We could just allocate this locally inside the Draw method, but it
        // is more efficient to reuse a single array, as this avoids creating
        // unnecessary garbage.
        //Matrix[] boneTransforms;      ----Using inherited matrix from Actor----


        // Current animation positions.
        float LwheelRotationValue;
        float RwheelRotationValue;
        float steerRotationValue;
        float turretRotationValue;
        float cannonRotationValue;
        float hatchRotationValue;
        public float turnDirection = 1.0f;

        public bool gamePlay = false;

        private Vector3 startingPos;
        String name;

        Cue soundCue;
        Cue treadsRollingCue;

        #endregion

        #region Bone Properties


        /// <summary>
        /// Gets or sets the wheel rotation amount.
        /// </summary>
        public float LWheelRotation
        {
            get { return LwheelRotationValue; }
            set { LwheelRotationValue = value; }
        }

        /// <summary>
        /// Gets or sets the wheel rotation amount.
        /// </summary>
        public float RWheelRotation
        {
            get { return RwheelRotationValue; }
            set { RwheelRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the steering rotation amount.
        /// </summary>
        public float SteerRotation
        {
            get { return steerRotationValue; }
            set { steerRotationValue = value; }
        }



        /// <summary>
        /// Gets or sets the turret rotation amount.
        /// </summary>
        public float TurretRotation
        {
            get { return turretRotationValue; }
            set { turretRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the cannon rotation amount.
        /// </summary>
        public float CannonRotation
        {
            get { return cannonRotationValue; }
            set { cannonRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the entry hatch rotation amount.
        /// </summary>
        public float HatchRotation
        {
            get { return hatchRotationValue; }
            set { hatchRotationValue = value; }
        }

        #endregion


        /// <summary>
        /// Construtor for the Player Tank
        /// </summary>
        /// <param name="game">Reference to the Game</param>
        /// <param name="spawnPos">Spawning Position of the Tank</param>
        public PlayerTank(Game game, Vector3 spawnPos)
            : base(game)
        {
            sMeshToLoad = "playerTank";
            startingPos = spawnPos;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
            Scale = 0.30f;
            WorldPosition = startingPos;
         
            COLLISION_IDENTIFIER = CollisionIdentifier.PLAYER_TANK;

        }

        /// <summary>
        /// Loads the tank model.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            tankModel = ActorModel;  

            // Look up shortcut references to the bones we are going to animate.
            leftBackWheelBone = tankModel.Bones["l_back_wheel_geo"];
            rightBackWheelBone = tankModel.Bones["r_back_wheel_geo"];
            leftFrontWheelBone = tankModel.Bones["l_front_wheel_geo"];
            rightFrontWheelBone = tankModel.Bones["r_front_wheel_geo"];
            leftSteerBone = tankModel.Bones["l_steer_geo"];
            rightSteerBone = tankModel.Bones["r_steer_geo"];
            turretBone = tankModel.Bones["turret_geo"];
            cannonBone = tankModel.Bones["canon_geo"];
            hatchBone = tankModel.Bones["hatch_geo"];

            // Store the original transform matrix for each animating bone.
            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;
            leftSteerTransform = leftSteerBone.Transform;
            rightSteerTransform = rightSteerBone.Transform;
            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;
            hatchTransform = hatchBone.Transform;

            // Allocate the transform matrix array.
            boneTransforms = new Matrix[tankModel.Bones.Count];



        }

        protected override void UnloadContent()
        {
            Console.Out.WriteLine("Player_tank is unloading content");
            base.UnloadContent();
        }

        public void ManualUnload()
        {
            UnloadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            base.updateWorldTransform();

            // Set the world matrix as the root transform of the model.
            tankModel.Root.Transform = worldTransform;

            // Calculate matrices based on the current animation position.
            Matrix LwheelRotation = Matrix.CreateRotationX(LwheelRotationValue);
            Matrix RwheelRotation = Matrix.CreateRotationX(RwheelRotationValue);
            Matrix steerRotation = Matrix.CreateRotationY(steerRotationValue);
            Matrix turretRotation = Matrix.CreateRotationY(turretRotationValue);
            Matrix cannonRotation = Matrix.CreateRotationX(cannonRotationValue);
            Matrix hatchRotation = Matrix.CreateRotationX(hatchRotationValue);

            // Apply matrices to the relevant bones.
            leftBackWheelBone.Transform = LwheelRotation * leftBackWheelTransform;
            rightBackWheelBone.Transform = RwheelRotation * rightBackWheelTransform;
            leftFrontWheelBone.Transform = LwheelRotation * leftFrontWheelTransform;
            rightFrontWheelBone.Transform = RwheelRotation * rightFrontWheelTransform;
            leftSteerBone.Transform = steerRotation * leftSteerTransform;
            rightSteerBone.Transform = steerRotation * rightSteerTransform;
            turretBone.Transform = turretRotation * turretTransform;
            cannonBone.Transform = cannonRotation * cannonTransform;
            hatchBone.Transform = hatchRotation * hatchTransform;

            // Look up combined bone matrices for the entire model.
            tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            GraphicsDevice.RenderState.DepthBufferEnable = true;

            // Draw the model.
            foreach (ModelMesh mesh in tankModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index];
                    if (gamePlay)
                    {
                        effect.View = GameplayScreen.CameraMatrix;
                        effect.Projection = GameplayScreen.ProjectionMatrix;
                    }
                    else
                    {
                        effect.View = BackgroundScreen.CameraMatrix;
                        effect.Projection = BackgroundScreen.ProjectionMatrix;
                    }

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.AmbientLightColor = Color.White.ToVector3();
                }

                mesh.Draw();
            }
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
            //Console.Out.WriteLine(WorldPosition);
            //WorldBounds.Center = WorldPosition + new Vector3(0,0, turretTransform.Translation.Z);
            //Console.Out.WriteLine(WorldBounds.Center);
            //WorldBounds.Radius = ModelBounds.Radius * Scale;
            if (gamePlay && soundCue == null)
            {
                soundCue = GameplayScreen.Instance.audioManager.soundSoundBank.GetCue("TankIdle");
                soundCue.Play();
            }
            if (gamePlay && soundCue != null)
            {
                if (Velocity == Vector3.Zero)
                {
                    name = soundCue.Name;
                    if (soundCue.IsStopped && name == "TankSlowDown")
                    {
                        soundCue.Stop(AudioStopOptions.Immediate);
                        soundCue = GameplayScreen.Instance.audioManager.soundSoundBank.GetCue("TankIdle");
                        soundCue.Play();
                    }
                    if (soundCue.IsStopped || name == "TankEngineMoving" || name == "TankSpeedUp")
                    {
                        soundCue = GameplayScreen.Instance.audioManager.soundSoundBank.GetCue("TankSlowDown");
                        //soundCue = GameplayScreen.Instance.audioManager.Play3DCue("TankSlowDown",this);
                        //soundCue.Play();
                    }
                }
                else
                {
                    name = soundCue.Name;
                    if (soundCue.IsStopped || name == "TankIdle"|| name =="TankSlowDown")
                    {
                        if (name == "TankIdle" || name == "TankSlowDown")
                        {
                            soundCue.Stop(AudioStopOptions.Immediate);

                            soundCue = GameplayScreen.Instance.audioManager.soundSoundBank.GetCue("TankSpeedUp");
                            soundCue.Play();

                        }
                        else
                        {
                            soundCue.Stop(AudioStopOptions.Immediate);
                            soundCue = GameplayScreen.Instance.audioManager.soundSoundBank.GetCue("TankEngineMoving");
                            soundCue.Play();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks collision between this actor and the given actor.
        /// </summary>
        /// <param name="a">Actor to check collision with.</param>
        /// <returns>True if the two actors are colliding.</returns>
        public override bool checkCollision(Actor a)
        {
            if (a.COLLISION_IDENTIFIER == CollisionIdentifier.BUILDING)
            {
                Building b = (Building)a;
                if (buildingIntersect(b))
                {
                    System.Console.Out.WriteLine("Collision with Building Detected");
                    return true;
                }
                return false;
            }
            else if (a.COLLISION_IDENTIFIER == CollisionIdentifier.AI_TANK)
            {
                if (a.WorldBounds.Intersects(WorldBounds))
                    return true;
                return false;
            }
            else 
                return false;
        }

        /// <summary>
        /// Resolves collision based on defined behaviors.
        /// </summary>
        /// <param name="a">Actor with which it is currently colliding.</param>
        public override void collide(Actor a)
        {
            if (a.COLLISION_IDENTIFIER == CollisionIdentifier.AI_TANK)
            {
                Velocity = new Vector3(0.0f, 0.0f, 0.0f);
                System.Console.Out.WriteLine("TANKS CRASHED!");
            }
            else if (a.COLLISION_IDENTIFIER == CollisionIdentifier.BUILDING)
            {
                Building b = (Building)a;

                Plane wall = findIntersectingPlane(b);

                Vector3 wallNormal = wall.Normal;
                wallNormal.Normalize();
                Vector3 perpVelComp = Vector3.Dot(Velocity, wallNormal * -1.0f) * wallNormal;

                Velocity -= perpVelComp;
                
                System.Console.Out.WriteLine("Should be Working");
                //Console.Out.WriteLine(b.WorldPosition);
            }
            else if (a.COLLISION_IDENTIFIER == CollisionIdentifier.SHELL)
            {
                Projectile temp = (Projectile)a;
                if (a.dead)
                {
                    CurrentHealth -= temp.Damage;
                    System.Console.Out.WriteLine("Ouch");
                }
                
            }
        }

        /// <summary>
        /// Checks if the player is intersecting with a building.
        /// </summary>
        /// <param name="b">Building object to check intersection against.</param>
        /// <param name="wall">out variable of type Plane.</param>
        /// <returns></returns>
        public bool buildingIntersect(Building b)
        {
            float minX = Math.Min(b.WorldBoundsBox.Min.X, b.WorldBoundsBox.Max.X);
            float minZ = Math.Min(b.WorldBoundsBox.Min.Z, b.WorldBoundsBox.Max.Z);
            float maxX = Math.Max(b.WorldBoundsBox.Min.X, b.WorldBoundsBox.Max.X);
            float maxZ = Math.Max(b.WorldBoundsBox.Min.Z, b.WorldBoundsBox.Max.Z);
          

            if (WorldPosition.X >= (minX - WorldBounds.Radius) && WorldPosition.X <= (maxX + WorldBounds.Radius))
            {
                if (WorldPosition.Z >= (minZ - WorldBounds.Radius) && WorldPosition.Z <= (maxZ + WorldBounds.Radius))
                {
                    
                    //Console.Out.WriteLine(b.WorldBoundsBox);
                    //Console.Out.WriteLine(WorldPosition);
                    return true;
                }
            }

            return false;
        }

        public Plane findIntersectingPlane(Building b)
        {

            Vector4 firstClosest = new Vector4();
            Vector4 secondClosest = new Vector4();

            Vector3[] corners = b.WorldBoundsBox.GetCorners();
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 temp = corners[i];
                float tempd = distanceSquared(new Vector2(temp.X, temp.Z), new Vector2(WorldPosition.X, WorldPosition.Z));

                if (firstClosest.W > tempd || firstClosest.W == 0)
                {
                    firstClosest = new Vector4(temp, tempd);
                }
                else if (secondClosest.W > tempd || secondClosest.W == 0)
                {
                    secondClosest = new Vector4(temp, tempd);
                }
            }

            firstClosest.W = (float)Math.Sqrt(firstClosest.W);
            secondClosest.W = (float)Math.Sqrt(secondClosest.W);
            Vector3 arbitrary = new Vector3();

            Random rand = new Random();



            if (firstClosest.X == secondClosest.X)
            {
                arbitrary.X = firstClosest.X;
                arbitrary.Z = rand.Next((int)Math.Min(firstClosest.Z, secondClosest.Z), (int)Math.Max(firstClosest.Z, secondClosest.Z));
            }
            else
            {
                arbitrary.Z = firstClosest.Z;
                arbitrary.X = rand.Next((int)Math.Min(firstClosest.X, secondClosest.X), (int)Math.Max(firstClosest.X, secondClosest.X));
            }

            return new Plane(new Vector3(firstClosest.X, firstClosest.Y, firstClosest.Z), arbitrary, new Vector3(secondClosest.X, secondClosest.Y, secondClosest.Z));

        }


        /// <summary>
        /// Computes the distance squared between two points.
        /// </summary>
        /// <param name="p1">Vector2 point 1.</param>
        /// <param name="p2">Vector2 point 2.</param>
        /// <returns></returns>
        public float distanceSquared(Vector2 p1, Vector2 p2)
        {
            return (p2.X - p1.X) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p2.Y - p1.Y);
        }

    }
}