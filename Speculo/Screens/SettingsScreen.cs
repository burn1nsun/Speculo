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

        Game1 game;

        Button changeResolution;
        Button fullScreenToggle;
        Button incrementVolume;
        Button decreaseVolume;
        Button back;
        List<Control> Controls = new List<Control>();

        int centerWidth;
        int centerHeight;
        private Texture2D bgTexture;
        private Rectangle bgRectangle;

        Texture2D texture;
        MouseState presentMouse;

        private SoundEffect clickSound;
        private SoundEffect hoverSound;
        private SoundEffect backSound;
        private Texture2D hoverTexture;
        private Rectangle buttonHoverRectangle;

        public SettingsScreen(Game1 game)
        {
            texture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Buttons/button1");
            bgTexture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Backgrounds/settingsbg");
            hoverTexture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Buttons/buttonOverlay");
            initializeButtons(game);

            clickSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/menuclick");
            backSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/menuback");
            hoverSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/MenuHit");

            this.game = game;

        }

        public void initializeButtons(Game1 game)
        {
            centerWidth = sharedVariables.GraphicsManager.PreferredBackBufferWidth / 2;
            centerHeight = sharedVariables.GraphicsManager.PreferredBackBufferHeight / 2;

            changeResolution = new Button(game.Content, sharedVariables.GraphicsManager.PreferredBackBufferWidth + " x " + sharedVariables.GraphicsManager.PreferredBackBufferHeight, new Rectangle(centerWidth - texture.Width / 2, centerHeight - 200, texture.Width, texture.Height), texture);
            fullScreenToggle = new Button(game.Content, "Fullscreen", new Rectangle(centerWidth - texture.Width / 2, centerHeight - 100, texture.Width, texture.Height), texture);
            incrementVolume = new Button(game.Content, "Volume+", new Rectangle(centerWidth - texture.Width / 2, centerHeight, texture.Width, texture.Height), texture);
            decreaseVolume = new Button(game.Content, "Volume-", new Rectangle(centerWidth - texture.Width / 2, centerHeight + 100, texture.Width, texture.Height), texture);
            back = new Button(game.Content, "Back", new Rectangle(centerWidth - texture.Width / 2, centerHeight + 200, texture.Width, texture.Height), texture);

            Controls.Clear();      
            Controls.Add(changeResolution);
            Controls.Add(fullScreenToggle);
            Controls.Add(incrementVolume);
            Controls.Add(decreaseVolume);
            Controls.Add(back);
            
            bgRectangle = new Rectangle(0, 0, sharedVariables.GraphicsManager.PreferredBackBufferWidth, sharedVariables.GraphicsManager.PreferredBackBufferHeight);
        }

        public void adjustToResolution(GameTime gameTime)
        {
            bool wasFullScreen = sharedVariables.GraphicsManager.IsFullScreen;
            if(wasFullScreen)
            {
                game.ToggleFullScreen();
            }
            game.UpdateScreenResolution((int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y);
            
            changeResolution.Text = sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X + " x " + sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y;

            initializeButtons(game);
            sharedVariables.GamePlay.initialize(gameTime);
            sharedVariables.GamePlay.CharacterClass.initialize();
            game.MenuScreen.initializeButtons(game);
            game.GameScreen.initialize();
            //game.Window.Position = new Point(10, 10);
            sharedVariables.Hud.Initialize();

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

            onButtonHover();


            if (changeResolution.IsLeftClicked)
            {
                clickSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                sharedVariables.ScreenSizeIndex++;
                if(sharedVariables.ScreenSizeIndex == sharedVariables.ScreenSizes.Count)
                {
                    sharedVariables.ScreenSizeIndex = 0;
                }
                adjustToResolution(gameTime);
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
        private void onButtonHover()
        {
            foreach(Control btn in Controls)
            {
                if (btn.IsMouseOver)
                {
                    buttonHoverRectangle = new Rectangle(btn.Rectangle.X, btn.Rectangle.Y, btn.Rectangle.Width, btn.Rectangle.Height);
                    if (!btn.HoverSoundPlayed && btn.IsMouseOver)
                    {
                        btn.HoverSoundPlayed = true;
                        hoverSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                    }
                }
                else
                {
                    btn.HoverSoundPlayed = false;
                }
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(bgTexture, bgRectangle, Color.White);
            foreach (Control control in Controls)
            {
                control.Draw(spriteBatch);
                if (control.IsMouseOver)
                {
                    spriteBatch.Draw(hoverTexture, buttonHoverRectangle, Color.White);
                }
            }
            spriteBatch.End();
        }
    }
}
