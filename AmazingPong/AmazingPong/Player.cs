using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PongGame
{
    class Player
    {
        Texture2D playerTexture;

        public Vector2 Position;

        public bool Active;

        public int Score;

        public int Width
        {
            get { return playerTexture.Width; }
        }

        public int Height
        {
            get { return playerTexture.Height; }
        }

        public void Initialize(Texture2D playerTexture, Vector2 Position)
        {
            this.playerTexture = playerTexture;
            this.Position = Position;

            Active = true;
            Score = 0;
        }

        public void Update()
        {    
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                spriteBatch.Draw(playerTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
        }
        
    }
}
