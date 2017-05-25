using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speculo.GameplayClasses
{
    public class HUD
    {
        Utility.SharedVariables sharedVariables = Utility.SharedVariables.Instance;


        private Vector2 boundsPosition;
        private Rectangle boundsRectangle;
        private Texture2D boundsTexture;

        public bool ShowHud { get; set; }

        public Rectangle BoundsRectangle
        {
            get { return boundsRectangle; }
            set { boundsRectangle = value; }
        }

        public HUD()
        {
            boundsTexture = sharedVariables.Content.Load<Texture2D>("Textures/bounds");

            boundsPosition = new Vector2(0, 0);
            boundsRectangle = new Rectangle ((int)boundsPosition.X, (int)boundsPosition.Y, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y);
        }

        public void LoadContent(ContentManager Content)
        {
            //hudFont = Content.Load<SpriteFont>("Courier New");
        }

        public void Update(GameTime gametime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (ShowHud)
            {
                spriteBatch.Draw(boundsTexture, boundsRectangle, Color.White);
            }
        }
    }
}
