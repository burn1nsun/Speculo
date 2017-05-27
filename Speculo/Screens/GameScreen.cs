using Microsoft.Xna.Framework;
using Speculo.CharacterClasses;
using Speculo.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Speculo.UserControls;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Speculo.Screens
{
    public class GameScreen : BaseScreen
    {
        SharedVariables sharedVariables = SharedVariables.Instance;

        Game1 game;

        public GameScreen(Game1 game)
        {
            this.game = game;
            Content = game.Content;
            LoadContent();

            paused = false;
            initialize();
        }

        //boundsrectangle

        private Vector2 boundsPosition;
        private Rectangle boundsRectangle;
        private Texture2D boundsTexture;

        public Rectangle BoundsRectangle
        {
            get { return boundsRectangle; }
            set { boundsRectangle = value; }
        }

        //Pause
        int centerWidth;
        int centerHeight;

        List<Control> Controls = new List<Control>();

        bool paused;
        Texture2D pausedTexture;
        Rectangle pausedRectangle;

        Texture2D buttonTexture;
        Button btnContinue, btnRetry, btnQuit;

        private SoundEffect menuPlaySound;
        private SoundEffect clickSound;
        private SoundEffect hoverSound;
        private SoundEffect backSound;

        void LoadContent()
        {
            boundsTexture = Content.Load<Texture2D>("Textures/bounds");
            pausedTexture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Backgrounds/pausebg");
            buttonTexture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Buttons/button1");

            menuPlaySound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/menuplay");
            clickSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/menuclick");
            hoverSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/MenuHit");
            backSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/menuback");
        }

        public void initialize()
        {
            centerWidth = sharedVariables.GraphicsManager.PreferredBackBufferWidth / 2;
            centerHeight = sharedVariables.GraphicsManager.PreferredBackBufferHeight / 2;

            boundsPosition = new Vector2(0, 0);
            boundsRectangle = new Rectangle((int)boundsPosition.X, (int)boundsPosition.Y, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y);

            pausedRectangle = new Rectangle(0, 0, pausedTexture.Width, pausedTexture.Height);

            btnContinue = new Button(sharedVariables.Content, "Continue", new Rectangle(centerWidth - buttonTexture.Width / 2, centerHeight - 100, buttonTexture.Width, buttonTexture.Height), buttonTexture);
            btnRetry = new Button(sharedVariables.Content, "Retry", new Rectangle(centerWidth - buttonTexture.Width / 2, centerHeight, buttonTexture.Width, buttonTexture.Height), buttonTexture);
            btnQuit = new Button(sharedVariables.Content, "Back to main menu", new Rectangle(centerWidth - buttonTexture.Width / 2, centerHeight + 100, buttonTexture.Width, buttonTexture.Height), buttonTexture);

            Controls.Clear();

            Controls.Add(btnContinue);
            Controls.Add(btnRetry);
            Controls.Add(btnQuit);

        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            if(!paused)
            {
                sharedVariables.GamePlay.CharacterClass.Update(gameTime);
                sharedVariables.GamePlay.Update(gameTime);

                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    paused = true;
                }
            }
            else if (paused)
            {
                if(btnContinue.IsLeftClicked)
                {
                    menuPlaySound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                    paused = false;
                }

                if (btnRetry.IsLeftClicked)
                {
                    clickSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                    paused = false;
                    sharedVariables.GamePlay.initialize();
                }

                if (btnQuit.IsLeftClicked)
                {
                    backSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                    this.IsActive = false;
                    paused = false;
                    game.MenuScreen.IsActive = true;
                }
                foreach (Control control in Controls)
                {
                    control.Update(mouse);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(boundsTexture, boundsRectangle, Color.White);
            sharedVariables.GamePlay.CharacterClass.Draw(spriteBatch);

            foreach(Enemy enemy in sharedVariables.GamePlay.enemyList)
            {
                enemy.Draw(spriteBatch);
            }

            //pause
            if(paused)
            {
                spriteBatch.Draw(pausedTexture, pausedRectangle, Color.White);
                foreach (Control control in Controls)
                {
                    control.Draw(spriteBatch);
                }
            }
            spriteBatch.End();
        }
    }
}
