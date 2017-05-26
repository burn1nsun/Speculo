using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speculo.UserControls
{
    public class Button : Control
    {
        public Button(ContentManager Content, string newText, Rectangle newRectangle, Texture2D texture)
        {
            Utility.SharedVariables sharedVariables = Utility.SharedVariables.Instance;
            this.texture = texture;
            
            font = sharedVariables.Hud.hudFont;

            text = newText;

            Rectangle = newRectangle;

            IsVisible = true;
            IsEnabled = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(IsVisible)
            {
                spriteBatch.Draw(texture, Rectangle, Color.White);

                if(text.Length > 0)
                {
                    float x = font.MeasureString(text).X / 2;
                    float y = font.MeasureString(text).Y / 2;

                    Vector2 fPosition = new Vector2((X + (Width / 2)) - x, (Y + (Height / 2)) - y);

                    spriteBatch.DrawString(font, text, fPosition, Color.White);
                }
            }
        }
    }
}
