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


namespace Battlezone.BattlezoneObjects.HUD
{
    /// <summary>
    /// This is a game component that implements IDrawable.
    /// </summary>
    public class WeaponSelect : DrawableGameComponent
    {

        SpriteBatch mBatch;

        private Texture2D turretSelect;
        private Texture2D missileSelect;

        //(

        public WeaponSelect(Game game)
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

            mBatch = new SpriteBatch(Game.GraphicsDevice);
            ContentManager aLoader = new ContentManager(Game.Services, "Content");

            turretSelect = aLoader.Load<Texture2D>("TurretSelect") as Texture2D;
            missileSelect = aLoader.Load<Texture2D>("MissileSelect2") as Texture2D;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        /// <summary>
        /// Allows the game component to Draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            mBatch.Begin();
            mBatch.Draw(turretSelect, new Rectangle(392, 708, 120, 60), Color.White);
            mBatch.Draw(missileSelect, new Rectangle(512, 708, 120, 60), Color.White);
            mBatch.End();

            base.Draw(gameTime);
        }
    }
}