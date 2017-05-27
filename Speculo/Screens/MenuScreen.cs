using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Speculo.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Speculo.Screens
{
    public class MenuScreen : BaseScreen
    {
        Utility.SharedVariables sharedVariables = Utility.SharedVariables.Instance;
        Game1 game;

        Button btnPlay;
        Button btnSettings;
        Button btnQuit;
        List<Control> Controls = new List<Control>();

        int centerWidth;
        int centerHeight;
        
        Texture2D texture;
        Texture2D bgTexture;
        Rectangle bgRectangle;

        private SoundEffect menuPlaySound;
        private SoundEffect clickSound;
        private SoundEffect hoverSound;
        private SoundEffect backSound;

        MouseState presentMouse;

        public MenuScreen(Game1 game)
        {
            this.texture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Buttons/button1");
            bgTexture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Backgrounds/menubg");
            initializeButtons(game);
            menuPlaySound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/menuplay");
            clickSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/menuclick");
            hoverSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/MenuHit");
            backSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/menuback");
        }

        public void initializeButtons(Game1 game)
        {
            centerWidth = sharedVariables.GraphicsManager.PreferredBackBufferWidth / 2;
            centerHeight = sharedVariables.GraphicsManager.PreferredBackBufferHeight / 2;

            this.game = game;
            
            btnPlay = new Button(game.Content, "Play", new Rectangle(centerWidth - texture.Width / 2, centerHeight - 100, texture.Width, texture.Height), texture);
            btnSettings = new Button(game.Content, "Settings", new Rectangle(centerWidth - texture.Width / 2, centerHeight, texture.Width, texture.Height), texture);
            btnQuit = new Button(game.Content, "Quit", new Rectangle(centerWidth - texture.Width / 2, centerHeight + 100, texture.Width, texture.Height), texture);

            Controls.Clear();

            Controls.Add(btnPlay);
            Controls.Add(btnSettings);
            Controls.Add(btnQuit);

            bgRectangle = new Rectangle(0, 0, sharedVariables.GraphicsManager.PreferredBackBufferWidth, sharedVariables.GraphicsManager.PreferredBackBufferHeight);
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
                sharedVariables.GamePlay.initialize();
                this.IsActive = false;
                game.GameScreen.IsActive = true;
                menuPlaySound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
            }

            if (btnSettings.IsLeftClicked)
            {
                clickSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                this.IsActive = false;
                game.SettingsScreen.IsActive = true;
            }

            if (btnQuit.IsLeftClicked)
            {
                backSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                game.Exit();
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(bgTexture, bgRectangle, Color.White);
            foreach(Control control in Controls)
            {
                control.Draw(spriteBatch);
            }
            spriteBatch.End();
        }
    }
}
