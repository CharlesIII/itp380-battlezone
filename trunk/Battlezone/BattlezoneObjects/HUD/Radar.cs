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
    public class Radar : DrawableGameComponent
    {
        SpriteBatch mBatch;

        private Texture2D PlayerDotImage;
        private Texture2D EnemyDotImage;
        private Texture2D RadarImage;

        // Local coords of the radar image's center, used to offset image when being drawn
        private Vector2 RadarImageCenter;

        // Distance that the radar can "see"
        private const float RadarRange = 2500.0f;
        private const float RadarRangeSquared = RadarRange * RadarRange;

        // Radius of radar circle on the screen
        private const float RadarScreenRadius = 150.0f;

        // This is the center position of the radar hud on the screen. 
        static Vector2 RadarCenterPos = new Vector2(900.0f, 75.0f);

        /// <summary>
        /// Constructs the Radar on the HUD.
        /// </summary>
        /// <param name="game"> A reference to the Game.</param>
        public Radar(Game game)
            : base(game)
        {
            
        }
        /// <summary>
        /// Initialize Function
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            mBatch = new SpriteBatch(Game.GraphicsDevice);

            ContentManager aLoader = new ContentManager(Game.Services, "Content");

            PlayerDotImage = aLoader.Load<Texture2D>("yellowDotSmall") as Texture2D;
            EnemyDotImage = aLoader.Load<Texture2D>("redDotSmall") as Texture2D;
            RadarImage = aLoader.Load<Texture2D>("blackDotLarge") as Texture2D;

            RadarImageCenter = new Vector2(RadarImage.Width * 0.5f, RadarImage.Height * 0.5f);

        }

        /// <summary>
        /// Draw Function
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public override void Draw(GameTime gameTime)
        {
            Vector3 playerFwd = GameplayScreen.Instance.getPlayer().GetWorldFacing();
            Vector3 playerPos = GameplayScreen.Instance.getPlayer().GetWorldPosition();

            mBatch.Begin();
            // The last parameter of the color determines how transparent the radar circle will be
            mBatch.Draw(RadarImage, RadarCenterPos, null, new Color(100, 100, 100, 150), 0.0f, RadarImageCenter, 1.0f, SpriteEffects.None, 0.0f);

            float playerForwardRadians = (float)Math.Acos(Vector2.Dot(new Vector2(playerFwd.Z, playerFwd.X), new Vector2(0.0f, 1.0f)));
            // If enemy is in range
            foreach (AITank thisEnemy in GameplayScreen.Instance.Enemies)
            {
                Vector2 diffVect = new Vector2(thisEnemy.WorldPosition.Z - playerPos.Z, thisEnemy.WorldPosition.X - playerPos.X);
                float distance = diffVect.LengthSquared();

                // Check if enemy is within RadarRange
                if (distance < RadarRangeSquared)
                {
                    // Scale the distance from world coords to radar coords
                    diffVect *= RadarScreenRadius / RadarRange;

                    // We rotate each point on the radar so that the player is always facing UP on the radar
                    diffVect = Vector2.Transform(diffVect, Matrix.CreateRotationZ(playerForwardRadians));

                    // Offset coords from radar's center
                    diffVect += RadarCenterPos;

                    // We scale each dot so that enemies that are at higher elevations have bigger dots, and enemies
                    // at lower elevations have smaller dots.
                    float scaleHeight = 1.0f + ((thisEnemy.WorldPosition.Y - playerPos.Y) / 200.0f);

                    // Draw enemy dot on radar
                   mBatch.Draw(EnemyDotImage, diffVect, null, Color.White, 0.0f, new Vector2(0.0f, 0.0f), scaleHeight, SpriteEffects.None, 0.0f);
                }
            }

            // Draw player's dot last
            mBatch.Draw(PlayerDotImage, RadarCenterPos, Color.White);

            mBatch.End();

            base.Draw(gameTime);
        }
    }
}
