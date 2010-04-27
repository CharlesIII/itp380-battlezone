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
            ScreenManager.soundAudioEngine.Update();
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
            ScreenManager.soundAudioEngine.Update();
            if (gamePlay && soundCue == null)
            {
                soundCue = ScreenManager.soundSoundBank.GetCue("TankIdle");
                soundCue.Play();
            }
            if (gamePlay && soundCue != null)
            {
                if (Velocity == Vector3.Zero)
                {
                    name = soundCue.Name;
                    if (name == "TankEngineMoving" || name == "TankTreadRolling")
                    {
                        soundCue.Stop(AudioStopOptions.Immediate);
                    }
                    if (soundCue.IsStopped || name == "TankEngineMoving" || name == "TankTreadRolling")
                    {
                        soundCue = ScreenManager.soundSoundBank.GetCue("TankIdle");
                        soundCue.Play();
                    }
                }
                else
                {
                    name = soundCue.Name;
                    if (soundCue.IsStopped || name == "TankIdle")
                    {
                        if (name == "TankIdle")
                        {
                            soundCue.Stop(AudioStopOptions.Immediate);
                            soundCue = ScreenManager.soundSoundBank.GetCue("TankTreadRolling");
                            soundCue.Play();
                            soundCue.Stop(AudioStopOptions.Immediate);
                            soundCue = ScreenManager.soundSoundBank.GetCue("TankEngineMoving");
                            soundCue.Play();

                        }
                        else
                        {
                            soundCue = ScreenManager.soundSoundBank.GetCue("TankTreadRolling");
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
                if (b.WorldBoundsBox.Intersects(WorldBounds))
                    return true;
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
                Vector3 tank2building = b.WorldPosition - WorldPosition;

                Velocity += tank2building * -1.0f;
                //System.Console.Out.WriteLine(b.WorldBoundsBox);
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

    }
}