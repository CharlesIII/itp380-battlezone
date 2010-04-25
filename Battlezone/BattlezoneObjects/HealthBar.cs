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
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class HealthBar : DrawableGameComponent
    {

        //Health Bar
        SpriteBatch mBatch;
        Texture2D mHealthBar;


        public HealthBar(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        protected override void LoadContent()
        {
            //Load Tank's Health Bar
            mBatch = new SpriteBatch(this.Game.GraphicsDevice);
            ContentManager aLoader = new ContentManager(Game.Services, "Content");
            mHealthBar = aLoader.Load<Texture2D>("HealthBar2") as Texture2D;
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
        public override void Draw(GameTime gameTime)
        {
            Color myC;

            //Draw Health Bar
            mBatch.Begin();
            if (GameplayScreen.Instance.getPlayer().CurrentHealth < (0.25f * GameplayScreen.Instance.getPlayer().maxHealth))
            {
                myC = Color.Red;
            }
            else if (GameplayScreen.Instance.getPlayer().CurrentHealth < (0.60f * GameplayScreen.Instance.getPlayer().maxHealth))
            {
                myC = Color.GreenYellow;
            }
            else
            {
                myC = Color.Green;
            }
            
            //Draw the negative space for the health bar
            mBatch.Draw(mHealthBar, new Rectangle(this.Game.Window.ClientBounds.Width / 2 - mHealthBar.Width / 2, 30, mHealthBar.Width, 44), new Rectangle(0, 45, mHealthBar.Width, 44), Color.Gray);
            //Draw the current health level based on the current Health
            mBatch.Draw(mHealthBar, new Rectangle(this.Game.Window.ClientBounds.Width / 2 - mHealthBar.Width / 2, 30, (int)(mHealthBar.Width * (GameplayScreen.Instance.getPlayer().CurrentHealth / 100)), 44), new Rectangle(0, 45, mHealthBar.Width, 44), myC);
            mBatch.Draw(mHealthBar, new Rectangle(this.Game.Window.ClientBounds.Width / 2 - mHealthBar.Width / 2, 30, mHealthBar.Width, 44), new Rectangle(0, 0, mHealthBar.Width, 44), Color.White);
            mBatch.End();

            base.Draw(gameTime);
        }
    }
}