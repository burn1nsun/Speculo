using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Speculo.UserControls;
using Microsoft.Xna.Framework.Input;

namespace Speculo.Screens
{
    public class SettingsScreen : BaseScreen
    {
        Utility.SharedVariables sharedVariables = Utility.SharedVariables.Instance;

        Button changeResolution;
        Button fullScreenToggle;
        Button incrementVolume;
        Button decreaseVolume;
        Button back;
        

        MouseState presentMouse;
        Game1 game;

        List<Control> Controls = new List<Control>();

        public SettingsScreen(Game1 game)
        {
            int centerWidth = sharedVariables.GraphicsManager.PreferredBackBufferWidth / 2;
            int centerHeight = sharedVariables.GraphicsManager.PreferredBackBufferHeight / 2;

            this.game = game;
            Texture2D texture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Buttons/button1");
            changeResolution = new Button(game.Content, sharedVariables.GraphicsManager.PreferredBackBufferWidth + " x " + sharedVariables.GraphicsManager.PreferredBackBufferHeight, new Rectangle(centerWidth - texture.Width / 2, centerHeight - 200, texture.Width, texture.Height), texture);
            fullScreenToggle = new Button(game.Content, "Fullscreen", new Rectangle(centerWidth - texture.Width / 2, centerHeight - 100, texture.Width, texture.Height), texture);
            incrementVolume = new Button(game.Content, "Volume+", new Rectangle(centerWidth - texture.Width / 2, centerHeight, texture.Width, texture.Height), texture);
            decreaseVolume = new Button(game.Content, "Volume-", new Rectangle(centerWidth - texture.Width / 2, centerHeight + 100, texture.Width, texture.Height), texture);
            back = new Button(game.Content, "Back", new Rectangle(centerWidth - texture.Width / 2, centerHeight + 200, texture.Width, texture.Height), texture);

            Controls.Add(changeResolution);
            Controls.Add(fullScreenToggle);
            Controls.Add(incrementVolume);
            Controls.Add(decreaseVolume);
            Controls.Add(back);
        }

        public override void Update(GameTime gameTime)
        {
            presentMouse = Mouse.GetState();
            foreach (Control control in Controls)
            {
                control.Update(presentMouse);
            }

            if (changeResolution.IsLeftClicked)
            {
                sharedVariables.ScreenSizeIndex++;
                if(sharedVariables.ScreenSizeIndex == sharedVariables.ScreenSizes.Count)
                {
                    sharedVariables.ScreenSizeIndex = 0;
                }

                game.UpdateScreenResolution((int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y);
                changeResolution.Text = sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X + " x " + sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y;
            }

            if (fullScreenToggle.IsLeftClicked)
            {
                game.ToggleFullScreen();
            }

            if (incrementVolume.IsLeftClicked)
            {
                
            }

            if (decreaseVolume.IsLeftClicked)
            {

            }

            if (back.IsLeftClicked)
            {
                this.IsActive = false;
                game.MenuScreen.IsActive = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            foreach (Control control in Controls)
            {
                control.Draw(spriteBatch);
            }
            spriteBatch.End();
        }
    }
}
