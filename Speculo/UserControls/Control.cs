using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speculo.UserControls
{
    public class Control
    {
        protected MouseState presentMouse;
        protected MouseState pastMouse;

        protected bool hoverSoundPlayed;

        protected SpriteFont font;

        protected ContentManager Content;

        protected string text = "";

        public string Text
        {
            get { return text;  }
            set { text = value;  }
        }

        public Texture2D texture;

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        public bool HoverSoundPlayed
        {
            get { return hoverSoundPlayed; }
            set { hoverSoundPlayed = value; }
        }

        private Rectangle rectangle;
        public Rectangle Rectangle
        {
            get { return rectangle; }
            set { rectangle = value; }
        }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value;  }
        }

        public bool IsMouseOver
        {
            get
            {
                Rectangle rect = new Rectangle(presentMouse.X, presentMouse.Y, 1, 1);

                return IsVisible && rect.Intersects(Rectangle);
            }
        }

        protected List<Control> controls = new List<Control>();
        public List<Control> Controls
        {
            get { return controls;  }
        }

        public bool IsLeftClicked
        {
            get {  return IsMouseOver && (presentMouse.LeftButton == ButtonState.Released && pastMouse.LeftButton == ButtonState.Pressed);  }
        }

        public bool IsRightClicked
        {
            get { return IsMouseOver && (presentMouse.RightButton == ButtonState.Released && pastMouse.RightButton == ButtonState.Pressed); }
        }

        private Vector2 position = Vector2.Zero;
        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                rectangle.X = (int)position.X;
                rectangle.Y = (int)position.Y;
            }
        }

        public int Width
        {
            get { return Rectangle.Width; }
        }
        public int Height
        {
            get { return Rectangle.Height; }
        }

        public int X
        {
            get { return Rectangle.X; }
        }

        public int Y
        {
            get { return Rectangle.Y; }
        }

        public virtual void Update(MouseState mouse)
        {
            pastMouse = presentMouse;
            presentMouse = mouse;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
