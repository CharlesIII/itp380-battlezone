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
using Battlezone.Engine;
using Battlezone.BattlezoneObjects;
using System.Timers;


namespace Battlezone
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class SpawnManager : Microsoft.Xna.Framework.GameComponent
    {
        Utils.Timer timer;
        Random rng;
        int maxScreenX, maxScreenY;
        int count;
        Game myGame;
        PlayerTank player;
        PathFinder navPathFind;

        public SpawnManager(Game game,PlayerTank tank,PathFinder finder)
            : base(game)
        {
            // TODO: Construct any child components here
            timer = new Utils.Timer();
            rng = new Random();
            player = tank;
            navPathFind = finder;

            myGame = game;

            maxScreenX = Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2;
            maxScreenY = Game.GraphicsDevice.PresentationParameters.BackBufferHeight / 2;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            count=0;
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            if (count % 100 == 0 && count<1000)
            {
                AITank temp = new AITank(myGame, navPathFind, player.WorldPosition);
                //Enemies.Add(temp);
                myGame.Components.Add(temp);
                count++;
            }
            base.Update(gameTime);

        }
    }
}