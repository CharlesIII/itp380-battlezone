using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Utils
{
    public class FrameRateCounter : DrawableGameComponent
    {
        private ContentManager m_kContent;
        private SpriteBatch m_kSpriteBatch;
        private SpriteFont m_kFont;

        private Vector2 m_vPosition;

		private float m_fCurrentFrameRate;
        private float m_fHighestFrameRate = 0.0f;
        private float m_fLowestFrameRate = 0.0f;
        private const int numOfFrames = 100;

        private Queue<float> m_kLastFrames = new Queue<float>(numOfFrames);  
        
        public FrameRateCounter(Game game, Vector2 vPosition)
            : base(game)
        {
            m_kContent = new ContentManager(game.Services);
            m_kContent.RootDirectory = "Content";

            m_vPosition = vPosition;
            DrawOrder = 1000;
        }

        protected override void LoadContent()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)this.Game.Services.GetService(typeof(IGraphicsDeviceService));

            m_kSpriteBatch = new SpriteBatch(graphicsService.GraphicsDevice);
            m_kFont = m_kContent.Load<SpriteFont>("fpsfont");
        }
        
        protected override void UnloadContent()
        {
            m_kContent.Unload();
        }
        
        public override void Update(GameTime gameTime)
        {
            if (gameTime.ElapsedGameTime.Ticks != 0)
            {
                m_fCurrentFrameRate = 1 / ((float)gameTime.ElapsedGameTime.Ticks / System.TimeSpan.TicksPerMillisecond / 1000.0f);
                if (m_kLastFrames.Count < numOfFrames)
                    m_kLastFrames.Enqueue(m_fCurrentFrameRate);
                else
                {
                    m_kLastFrames.Dequeue();
                    m_kLastFrames.Enqueue(m_fCurrentFrameRate);
                }
            }
			//We can't use the below because our framerate can be SO HIGH that the ms value rounds to zero
			//m_fCurrentFrameRate = 1 / ((float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

			base.Update(gameTime);
        }
        
        public override void Draw(GameTime gameTime)
        {
            m_kSpriteBatch.Begin();
            
			// Color this based on the framerate
            Color DrawColor = Color.Green;
			if (m_fCurrentFrameRate < 15.0f)
                DrawColor = Color.Red;
			else if (m_fCurrentFrameRate < 30.0f)
                DrawColor = Color.Yellow;
            
            float frameRate = 0.0f;
            m_fLowestFrameRate = 0.0f;
            m_fHighestFrameRate = 0.0f;
            foreach (float fps in m_kLastFrames)
            {
                frameRate += fps;
                if (fps > m_fHighestFrameRate)
                    m_fHighestFrameRate = fps;
                else if (fps < m_fLowestFrameRate)
                    m_fLowestFrameRate = fps;
                else if (m_fLowestFrameRate == 0)
                    m_fLowestFrameRate = fps;
            }
            frameRate = frameRate / (float)m_kLastFrames.Count;

            m_kSpriteBatch.DrawString(m_kFont, "FPS: " + frameRate.ToString("f3"), m_vPosition, DrawColor);
            m_kSpriteBatch.DrawString(m_kFont, "Max FPS: " + m_fHighestFrameRate.ToString("f3"), new Vector2(m_vPosition.X, m_vPosition.Y + 20), DrawColor);
            m_kSpriteBatch.DrawString(m_kFont, "Min FPS: " + m_fLowestFrameRate.ToString("f3"), new Vector2(m_vPosition.X, m_vPosition.Y + 40), DrawColor);
            m_kSpriteBatch.End();

        }

		public void ResetFPSCount()
		{
            m_kLastFrames.Clear();
		}
    }
}
