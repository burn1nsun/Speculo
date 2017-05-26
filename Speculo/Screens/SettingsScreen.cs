using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Speculo.UserControls;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

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

        int centerWidth;
        int centerHeight;

        Texture2D texture;
        MouseState presentMouse;
        Game1 game;

        List<Control> Controls = new List<Control>();

        private SoundEffect clickSound;
        private SoundEffect hoverSound;
        private SoundEffect backSound;

        public SettingsScreen(Game1 game)
        {
            texture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Buttons/button1");
            initializeButtons(game);

            clickSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/menuclick");
            hoverSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/MenuHit");
            backSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/menuback");

        }

        public void initializeButtons(Game1 game)
        {
            centerWidth = sharedVariables.GraphicsManager.PreferredBackBufferWidth / 2;
            centerHeight = sharedVariables.GraphicsManager.PreferredBackBufferHeight / 2;

            this.game = game;
            
            changeResolution = new Button(game.Content, sharedVariables.GraphicsManager.PreferredBackBufferWidth + " x " + sharedVariables.GraphicsManager.PreferredBackBufferHeight, new Rectangle(centerWidth - texture.Width / 2, centerHeight - 200, texture.Width, texture.Height), texture);
            fullScreenToggle = new Button(game.Content, "Fullscreen", new Rectangle(centerWidth - texture.Width / 2, centerHeight - 100, texture.Width, texture.Height), texture);
            incrementVolume = new Button(game.Content, "Volume+", new Rectangle(centerWidth - texture.Width / 2, centerHeight, texture.Width, texture.Height), texture);
            decreaseVolume = new Button(game.Content, "Volume-", new Rectangle(centerWidth - texture.Width / 2, centerHeight + 100, texture.Width, texture.Height), texture);
            back = new Button(game.Content, "Back", new Rectangle(centerWidth - texture.Width / 2, centerHeight + 200, texture.Width, texture.Height), texture);

            if(Controls == null)
            {
                Controls.Add(changeResolution);
                Controls.Add(fullScreenToggle);
                Controls.Add(incrementVolume);
                Controls.Add(decreaseVolume);
                Controls.Add(back);
            } else
            {
                Controls.Clear();

                Controls.Add(changeResolution);
                Controls.Add(fullScreenToggle);
                Controls.Add(incrementVolume);
                Controls.Add(decreaseVolume);
                Controls.Add(back);
            }

        }

        public void adjustToResolution()
        {
            bool wasFullScreen = sharedVariables.GraphicsManager.IsFullScreen;
            if(wasFullScreen)
            {
                game.ToggleFullScreen();
            }
            game.UpdateScreenResolution((int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y);
            changeResolution.Text = sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X + " x " + sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y;

            initializeButtons(game);
            sharedVariables.Hud.Initialize();
            game.MenuScreen.initializeButtons(game);
            sharedVariables.GamePlay.initialize();
            sharedVariables.CharacterClass.initialize();

            if (wasFullScreen)
            {
                game.ToggleFullScreen();
            }

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

                clickSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                sharedVariables.ScreenSizeIndex++;
                if(sharedVariables.ScreenSizeIndex == sharedVariables.ScreenSizes.Count)
                {
                    sharedVariables.ScreenSizeIndex = 0;
                }
                adjustToResolution();
            }

            if (fullScreenToggle.IsLeftClicked)
            {
                clickSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                game.ToggleFullScreen();
            }

            if (incrementVolume.IsLeftClicked)
            {
                sharedVariables.SoundFxVolume = MathHelper.Clamp(sharedVariables.SoundFxVolume + 0.1f, 0.0f, 1.0f);
                clickSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                sharedVariables.Hud.volumeLevelChanged(gameTime);
            }

            if (decreaseVolume.IsLeftClicked)
            {
                sharedVariables.SoundFxVolume = MathHelper.Clamp(sharedVariables.SoundFxVolume - 0.1f, 0.0f, 1.0f);
                clickSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);

                sharedVariables.Hud.volumeLevelChanged(gameTime);
            }

            if (back.IsLeftClicked)
            {
                backSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
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
