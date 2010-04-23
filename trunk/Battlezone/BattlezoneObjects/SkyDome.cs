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
    public class SkyDome : Actor
    {

        #region Bone Fields

        TextureMaterial textureMaterial;
        GraphicsDevice device;
        protected ContentManager textureLoader;
        float rotationY;


        //bool alphaBlendEnable;
        //Blend sourceBlend;
        //Blend destinationBlend;
        CullMode cullMode;
        bool depthBufferEnable;
        bool depthBufferWriteEnable;
        //bool alphaTestEnable;
        //TextureAddressMode addressU;
        //TextureAddressMode addressV;
   

        #endregion



        public SkyDome(Game game,GraphicsDevice device)
            : base(game)
        {
            sMeshToLoad = "sphere";
            this.device = device;
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
            Scale = 100.0f;
        }

        /// <summary>
        /// Loads the tank model.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
            GetTextureMaterial("SkyDome", Vector2.One);
           
        }

        private void GetTextureMaterial(string textureFilename, Vector2 tile)
        {
            textureLoader = new ContentManager(Game.Services, "Content");
            Texture2D texture = textureLoader.Load<Texture2D>(textureFilename);
            textureMaterial = new TextureMaterial(texture, tile);
        }

        private void SetEffectMaterial(BasicEffect basicEffect, Matrix viewMatrix, Matrix projectionMatrix)
        {
            // Fix the skydome texture coordinate
            //GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            basicEffect.DiffuseColor = Color.White.ToVector3();

            // Texture Material
            basicEffect.Texture = textureMaterial.Texture;
            basicEffect.TextureEnabled = true;

            // Transformation
            //basicEffect.World = transformation.Matrix;
            basicEffect.World = worldTransform;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
        }

        public override void Update(GameTime gameTime)
        {
            rotationY += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.05f;

            worldTransform =
                Matrix.CreateScale(200) *
                //Matrix.CreateRotationX(-MathHelper.PiOver2) *
                Matrix.CreateRotationY(rotationY) *
                Matrix.CreateTranslation(GameplayScreen.Instance.Position);
        }

        private void SaveGraphicsDeviceState()
        {
            //alphaBlendEnable = device.RenderState.AlphaBlendEnable;
            //sourceBlend = device.RenderState.SourceBlend;
            //destinationBlend = device.RenderState.DestinationBlend;
            cullMode = device.RenderState.CullMode;
            depthBufferEnable = device.RenderState.DepthBufferEnable;
            depthBufferWriteEnable = device.RenderState.DepthBufferWriteEnable;
            //alphaTestEnable = device.RenderState.AlphaTestEnable;
            //addressU = device.SamplerStates[0].AddressU;
            //addressV = device.SamplerStates[0].AddressV;
        }

        private void RestoreGraphicsDeviceState()
        {
            //device.RenderState.AlphaBlendEnable = alphaBlendEnable;
            //device.RenderState.SourceBlend = sourceBlend;
            //device.RenderState.DestinationBlend = destinationBlend;
            device.RenderState.CullMode = cullMode;
            device.RenderState.DepthBufferEnable = depthBufferEnable;
            device.RenderState.DepthBufferWriteEnable = depthBufferWriteEnable;
            //device.RenderState.AlphaTestEnable = alphaTestEnable;
            //device.SamplerStates[0].AddressU = addressU;
            //device.SamplerStates[0].AddressV = addressV;
        }


        public override void Draw(GameTime gameTime)
        {

            SaveGraphicsDeviceState();

            //device.RenderState.AlphaBlendEnable = true;
            //device.RenderState.SourceBlend = Blend.SourceAlpha;
            //device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            device.RenderState.CullMode = CullMode.CullClockwiseFace;
            device.RenderState.DepthBufferEnable = true;
            device.RenderState.DepthBufferWriteEnable = true;
            //device.RenderState.AlphaTestEnable = false;
            //device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            //device.RenderState.FillMode = FillMode.WireFrame;
            foreach (ModelMesh modelMesh in ActorModel.Meshes)
            {
                // We are only rendering models with BasicEffect
                foreach (BasicEffect basicEffect in modelMesh.Effects)
                    SetEffectMaterial(basicEffect, GameplayScreen.CameraMatrix, GameplayScreen.ProjectionMatrix);

                modelMesh.Draw();
            }

            device.RenderState.FillMode = FillMode.Solid;
            RestoreGraphicsDeviceState();


        }



        public class TextureMaterial
        {

            Texture2D texture;
            Vector2 uvTile;

            #region Properties
            public Texture2D Texture { get { return texture; } set { texture = value; } }

            public Vector2 UVTile { get { return uvTile; } set { uvTile = value; } }
            #endregion

            public TextureMaterial() { }

            public TextureMaterial(Texture2D texture, Vector2 uvTile)
            {
                this.texture = texture;
                this.uvTile = uvTile;
            }

        }

    }
}