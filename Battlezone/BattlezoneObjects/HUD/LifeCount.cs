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
    public class LifeCount : DrawableGameComponent
    {

        SpriteBatch mBatch;

        private Texture2D turretSelect;

        public int life = 3;  //1 for Turret, 2 for Missile

        //(

        public LifeCount(Game game)
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

            turretSelect = aLoader.Load<Texture2D>("tank-display") as Texture2D;
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
            Color c = new Color(new Vector4(255, 255, 255, 100));

            mBatch.Draw(turretSelect, new Rectangle(30, 30, 70, 50), c);
            mBatch.DrawString(GameplayScreen.Instance.ScreenManager.Font, "x " + life, new Vector2(100, 30), Color.White);

            mBatch.End();

            base.Draw(gameTime);
        }

    }
}
