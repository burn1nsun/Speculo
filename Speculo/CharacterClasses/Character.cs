﻿using Microsoft.Xna.Framework;
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

        public Vector2 Position
        {
            get {  return this.position;  }
            set { this.position = value; }
        }
        public Rectangle Rectangle
        {
            get { return this.characterRectangle; }
            set { this.characterRectangle = value; }
        }

        public Texture2D Texture
        {
            get { return this.texture; }
            set { this.texture = value; }
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
            characterRectangle = new Rectangle((int)position.X, (int)position.Y, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 15, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 69);
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
            if(position.X + characterRectangle.Width > sharedVariables.GamePlay.PlayArea.Width + sharedVariables.GamePlay.PlayArea.X)
            {
                position.X = (sharedVariables.GamePlay.PlayArea.Width + sharedVariables.GamePlay.PlayArea.X) - characterRectangle.Width;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Rectangle((int)position.X, (int)position.Y, characterRectangle.Width, characterRectangle.Height), Color.White);
        }
    }
}
