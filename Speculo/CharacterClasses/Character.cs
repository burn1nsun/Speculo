using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Speculo.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speculo.CharacterClasses
{
    public class Character
    {
        private Texture2D texture;
        private Vector2 position;
        //private Vector2 velocity;
        //private Vector2 origin;

        private Rectangle rectangle;

        SharedVariables sharedVariables = SharedVariables.Instance;

        public Rectangle Rectangle
        {
            get { return this.rectangle; }
            set { this.rectangle = value; }
        }


        public Character()
        {
            if (texture == null)
            {
                texture = sharedVariables.Content.Load<Texture2D>("Textures/character");
            }

            position = new Vector2(0, 0);
            rectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            position.Y = sharedVariables.GraphicsManager.PreferredBackBufferHeight - (sharedVariables.GraphicsManager.PreferredBackBufferHeight / 100 * 10);

        }
        public void Update(GameTime gameTime)
        {
            
            MouseState newState = Mouse.GetState();
            position.X = newState.X - texture.Width / 2 ;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, rectangle, Color.White);
        }
    }
}
