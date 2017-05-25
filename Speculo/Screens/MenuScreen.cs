using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Speculo.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Speculo.Screens
{
    public class MenuScreen : BaseScreen
    {
        Utility.SharedVariables sharedVariables = Utility.SharedVariables.Instance;
        Button btnPlay;
        Button btnSettings;
        Button btnQuit;

        List<Control> Controls = new List<Control>();

        MouseState presentMouse;
        Game1 game;

        public MenuScreen(Game1 game)
        {
            int center = sharedVariables.GraphicsManager.PreferredBackBufferWidth / 2;

            this.game = game;
            Texture2D texture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Buttons/button1"); 
            btnPlay = new Button(game.Content, "Play", new Rectangle(center - texture.Width / 2, 100, texture.Width, texture.Height), texture);
            btnSettings = new Button(game.Content, "Settings", new Rectangle(center - texture.Width / 2, 150, texture.Width, texture.Height), texture);
            btnQuit = new Button(game.Content, "Quit", new Rectangle(center - texture.Width / 2, 200, texture.Width, texture.Height), texture);

            Controls.Add(btnPlay);
            Controls.Add(btnQuit);
        }

        public override void Update(GameTime gameTime)
        {
            presentMouse = Mouse.GetState();
            foreach(Control control in Controls)
            {
                control.Update(presentMouse);
            }

            if(btnPlay.IsLeftClicked)
            {
                this.IsActive = false;
                game.GameScreen.IsActive = true;
            }

            if (btnSettings.IsLeftClicked)
            {

            }

            if (btnQuit.IsLeftClicked)
            {
                game.Exit();
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            foreach(Control control in Controls)
            {
                control.Draw(spriteBatch);
            }
            spriteBatch.End();
        }
    }
}
