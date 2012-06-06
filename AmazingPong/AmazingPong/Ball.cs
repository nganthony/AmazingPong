using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PongGame
{
    class Ball
    {
        public Texture2D ballTexture;

        public Vector2 Position;

        public bool Active;

        public int Width
        {
            get { return ballTexture.Width; }
        }

        public int Height
        {
            get { return ballTexture.Height; }
        }

        public void Initialize(Texture2D ballTexture, Vector2 Position)
        {
            this.ballTexture = ballTexture;
            this.Position = Position;

            Active = true;
        }

        public void Update()
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                spriteBatch.Draw(ballTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
        }
    }
}
