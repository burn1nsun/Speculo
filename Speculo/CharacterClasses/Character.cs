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

        private Rectangle characterRectangle;

        SharedVariables sharedVariables = SharedVariables.Instance;

        public Rectangle Rectangle
        {
            get { return this.characterRectangle; }
            set { this.characterRectangle = value; }
        }


        public Character()
        {
            if (texture == null)
            {
                texture = sharedVariables.Content.Load<Texture2D>("Textures/character");
            }
            initialize();

        }

        public void initialize()
        {
            position = new Vector2(0, 0);
            characterRectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            position.Y = sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y - (sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 100 * 10);
            position.X = sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 2;
        }

        public void Update(GameTime gameTime)
        {
            MouseState newState = Mouse.GetState();
            position.X = newState.X - texture.Width / 2;

            checkBounds();
        }

        public void checkBounds()
        {
            if(position.X < sharedVariables.GamePlay.PlayArea.X)
            {
                position.X = sharedVariables.GamePlay.PlayArea.X;
            }
            if(position.X + texture.Width > sharedVariables.GamePlay.PlayArea.Width + sharedVariables.GamePlay.PlayArea.X)
            {
                position.X = (sharedVariables.GamePlay.PlayArea.Width + sharedVariables.GamePlay.PlayArea.X) - texture.Width;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, characterRectangle, Color.White);
        }
    }
}
