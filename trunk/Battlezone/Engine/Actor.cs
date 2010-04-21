using System;
using System.Collections;
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


namespace Battlezone
{
    /// <summary>
    /// This is a modified version of the Actor class. It has been setup to take multiple models
    /// since we don't know how to construct a single model from multiple meshes.
    /// Drawing code has been updated to account for the change.
    /// </summary>
    public class Actor : Microsoft.Xna.Framework.DrawableGameComponent
    {
        protected Model ActorModel;
        protected ContentManager meshLoader;
        public string sMeshToLoad;

        public BoundingSphere ModelBounds;
        public BoundingSphere WorldBounds;

        public int COLLISION_IDENTIFIER;

        protected float fMass;
        protected float fTerminalVelocity;
        protected Vector3 vForce;
        public Vector3 Force
        {
            get
            {
                return vForce;
            }
            set
            {
                vForce = value;
            }
        }
        protected Vector3 vAcceleration;
        protected bool bPhysicsDriven;

        private bool m_bChanged;
        private float m_fScale;
        private float m_fRotAngle;
        public float RotAngle
        {
            get
            {
                return m_fRotAngle;
            }
            set
            {
                m_fRotAngle = value;
                m_bChanged = true;
            }
        }

        public float Scale
        {
            get
            {
                return m_fScale;
            }
            set
            {
                m_fScale = value;
                m_bChanged = true;
            }
        }

        protected Vector3 m_vPreviousWorldPosition;
        protected Vector3 m_vWorldPosition;
        public Vector3 WorldPosition
        {
            get
            {
                return m_vWorldPosition;
            }
            set
            {
                m_vWorldPosition = value;
                m_bChanged = true;
            }
        }

        private Vector3 m_vVelocity;
        public Vector3 Velocity
        {
            get
            {
                return m_vVelocity;
            }
            set
            {
                m_vVelocity = value;
                m_bChanged = true;
            }
        }
        
        private Quaternion m_Quat;
        public Quaternion Quat
        {
            get
            {
                return m_Quat;
            }
            set
            {
                m_Quat = value;
                m_bChanged = true;
            }
        }

        public Matrix worldTransform;
        protected Matrix[] boneTransforms;

        protected Utils.Timer timer;

        /// <summary>
        /// Constructs a new Actor.
        /// </summary>
        /// <param name="game">Reference to current game</param>
        public Actor(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            timer = new Utils.Timer();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            worldTransform = Matrix.Identity;
            m_fScale = 1.0f;
            m_vWorldPosition = new Vector3(0.0f, 0.0f, 0.0f);
            m_Quat = Quaternion.Identity;
            m_vVelocity = new Vector3(0.0f, 0.0f, 0.0f);
            m_fRotAngle = 0;
            bPhysicsDriven = false;
            fMass = 1;
            fTerminalVelocity = 1;
            vForce = new Vector3(0.0f, 0.0f, 0.0f);
            vAcceleration = new Vector3(0.0f, 0.0f, 0.0f);
            COLLISION_IDENTIFIER = CollisionIdentifier.NONCOLLIDING;
            base.Initialize();
        }

        /// <summary>
        /// Uses ContentManager to load Models specified by sMeshesToLoad and generate a BoundingSphere
        /// for the Actor.
        /// </summary>
        protected override void LoadContent()
        {
            meshLoader = new ContentManager(Game.Services, "Content");
            ActorModel = meshLoader.Load<Model>(sMeshToLoad);
            boneTransforms = new Matrix[ActorModel.Bones.Count];

            foreach (ModelMesh mesh in ActorModel.Meshes)
            {
                ModelBounds = BoundingSphere.CreateMerged(ModelBounds, mesh.BoundingSphere);
            }

            GameplayScreen.Instance.addActor(this);

            base.LoadContent();
        }

        /// <summary>
        /// Unloads content loaded by ContentManager.
        /// </summary>
        protected override void UnloadContent()
        {
            meshLoader.Unload();
            base.UnloadContent();
        }

        protected void updateWorldTransform()
        {
            if (m_bChanged)
            {
                worldTransform = Matrix.CreateScale(m_fScale);
                worldTransform *= Matrix.CreateFromQuaternion(m_Quat);
                worldTransform *= Matrix.CreateTranslation(m_vWorldPosition);

                WorldBounds.Center = m_vWorldPosition;
                WorldBounds.Radius = ModelBounds.Radius * m_fScale;
                m_bChanged = false;
            }
        }

        /// <summary>
        /// Handles drawing an Actor's models.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            
            updateWorldTransform();

            GraphicsDevice.RenderState.DepthBufferEnable = true;

            ActorModel.CopyAbsoluteBoneTransformsTo(boneTransforms);
            foreach (ModelMesh mesh in ActorModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * worldTransform;
                    effect.View = GameplayScreen.CameraMatrix;
                    effect.Projection = GameplayScreen.ProjectionMatrix;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.AmbientLightColor = Color.White.ToVector3();

                    //change later
                    /*
                    effect.SpecularColor = Color.Pink.ToVector3();
                    effect.SpecularPower = 4f;
                    effect.DirectionalLight0.Direction = GameplayScreen.DirLightDirection;
                    effect.DirectionalLight0.DiffuseColor = GameplayScreen.DiffuseColor;
                     */
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// Gets the World facing vector of the Actor
        /// </summary>
        /// <returns>A Vector3 representing the facing of the Actor</returns>
        public virtual Vector3 GetWorldFacing()
        {
            return worldTransform.Forward;
        }

        /// <summary>
        /// Gets the World position of the Actor
        /// </summary>
        /// <returns>A Vector3 containing the position of the Actor</returns>
        public Vector3 GetWorldPosition()
        {
            return worldTransform.Translation;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            float fDelta = gameTime.ElapsedGameTime.Ticks / System.TimeSpan.TicksPerMillisecond / 1000.0f;
            timer.Update(gameTime);

            if (bPhysicsDriven)
            {
                m_vVelocity += vAcceleration * fDelta / 2.0f;
                //TODO: Make sure this is being assigned by value, not by reference
                m_vPreviousWorldPosition = m_vWorldPosition;
                m_vWorldPosition += m_vVelocity * fDelta;
                vAcceleration = vForce / fMass;
                m_vVelocity += vAcceleration * fDelta / 2.0f;
                if (m_vVelocity.Length() >= fTerminalVelocity)
                {
                    m_vVelocity.Normalize();
                    m_vVelocity *= fTerminalVelocity;
                }
            }
            else
            {
                m_vWorldPosition += Vector3.Multiply(m_vVelocity, gameTime.ElapsedGameTime.Ticks / System.TimeSpan.TicksPerMillisecond / 1000.0f);

                //TODO: Add World Bound check so the player doesn't fall off the world
            }

            //old wrap around code; keeping it in case it's needed later
            /*
            if (m_vWorldPosition.X > Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2)
            {
                m_vWorldPosition.X = -Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2;
            }
            else if (m_vWorldPosition.X < -Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2)
            {
                m_vWorldPosition.X = Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2;
            }
            if (m_vWorldPosition.Y > Game.GraphicsDevice.PresentationParameters.BackBufferHeight / 2)
            {
                m_vWorldPosition.Y = -Game.GraphicsDevice.PresentationParameters.BackBufferHeight / 2;
            }
            else if (m_vWorldPosition.Y < -Game.GraphicsDevice.PresentationParameters.BackBufferHeight / 2)
            {
                m_vWorldPosition.Y = Game.GraphicsDevice.PresentationParameters.BackBufferHeight / 2;
            }
             */

            m_bChanged = true;
            
            base.Update(gameTime);
        }

        /// <summary>
        /// Removes the current Actor from the Game Components list as well as collision checking.
        /// </summary>
        protected virtual void removeSelf()
        {
            Game.Components.Remove(this);
            GameplayScreen.Instance.removeActor(this);
        }

        /// <summary>
        /// Resolves collision based on defined behaviors.
        /// </summary>
        /// <param name="a">Actor with which it is currently colliding.</param>
        public virtual void collide(Actor a)
        {
            //stub method, inheriting classes are expected to provide functionality
            //System.Console.Out.WriteLine("Inside Actor collide");
        }
    }
}