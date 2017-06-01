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
        private Rectangle buttonHoverRectangle;
        private Texture2D hoverTexture;
        private Texture2D levelPassTexture;
        private Texture2D levelFailTexture;

        void LoadContent()
        {
            boundsTexture = Content.Load<Texture2D>("Textures/bounds");
            pausedTexture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Backgrounds/pausebg");
            buttonTexture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Buttons/button1");
            hoverTexture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Buttons/buttonOverlay");
            levelPassTexture = sharedVariables.Content.Load<Texture2D>("Textures/Gameplay/sectionPass");
            levelFailTexture = sharedVariables.Content.Load<Texture2D>("Textures/Gameplay/sectionFail");

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
                sharedVariables.GamePlay.IsPlaying = true;

                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                     pause(gameTime);
                }
            }
            else if (paused)
            {
                sharedVariables.GamePlay.totalPauseTime = gameTime.TotalGameTime - sharedVariables.GamePlay.pauseTime + sharedVariables.GamePlay.lastPauseTime;

                if (btnContinue.IsMouseOver)
                {
                    buttonHoverRectangle = new Rectangle(btnContinue.Rectangle.X, btnContinue.Rectangle.Y, btnContinue.Rectangle.Width, btnContinue.Rectangle.Height);
                    if (!btnContinue.HoverSoundPlayed && btnContinue.IsMouseOver)
                    {
                        btnContinue.HoverSoundPlayed = true;
                        hoverSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                    }
                }
                else if (!btnContinue.IsMouseOver) { btnContinue.HoverSoundPlayed = false; }

                if (btnRetry.IsMouseOver)
                {
                    buttonHoverRectangle = new Rectangle(btnRetry.Rectangle.X, btnRetry.Rectangle.Y, btnRetry.Rectangle.Width, btnRetry.Rectangle.Height);
                    if (!btnRetry.HoverSoundPlayed && btnRetry.IsMouseOver)
                    {
                        btnRetry.HoverSoundPlayed = true;
                        hoverSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                    }
                }
                else if (!btnRetry.IsMouseOver) { btnRetry.HoverSoundPlayed = false; }


                if (btnQuit.IsMouseOver)
                {
                    buttonHoverRectangle = new Rectangle(btnQuit.Rectangle.X, btnQuit.Rectangle.Y, btnQuit.Rectangle.Width, btnQuit.Rectangle.Height);
                    if (!btnQuit.HoverSoundPlayed && btnQuit.IsMouseOver)
                    {
                        btnQuit.HoverSoundPlayed = true;
                        hoverSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                    }
                }
                else if (!btnQuit.IsMouseOver) { btnQuit.HoverSoundPlayed = false; }


                if (btnContinue.IsLeftClicked)
                {
                    menuPlaySound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                    unpause(gameTime);
                }

                if (btnRetry.IsLeftClicked)
                {
                    clickSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                    paused = false;
                    sharedVariables.GamePlay.initialize(gameTime);
                }

                if (btnQuit.IsLeftClicked)
                {
                    backSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                    this.IsActive = false;
                    paused = false;
                    game.MenuScreen.IsActive = true;
                    sharedVariables.GamePlay.IsPlaying = false;
                }
                foreach (Control control in Controls)
                {
                    control.Update(mouse);
                }
            }
        }

        public void pause(GameTime gameTime)
        {
            paused = true;
            sharedVariables.GamePlay.pause(gameTime);
        }

        public void unpause(GameTime gameTime)
        {
            paused = false;
            sharedVariables.GamePlay.unpause(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(boundsTexture, boundsRectangle, Color.White);
            sharedVariables.GamePlay.CharacterClass.Draw(spriteBatch);
            spriteBatch.Draw(sharedVariables.GamePlay.PlayAreaBorder, new Vector2(sharedVariables.GamePlay.PlayArea.X, sharedVariables.GamePlay.PlayArea.Y), Color.Red);

            foreach (Enemy enemy in sharedVariables.GamePlay.enemyList)
            {
                enemy.Draw(spriteBatch);
            }

            if (sharedVariables.GamePlay.LevelComplete && !sharedVariables.GamePlay.LevelFail)
            {
                spriteBatch.Draw(levelPassTexture,
                    /*new Vector2(sharedVariables.GraphicsManager.PreferredBackBufferWidth / 2 - levelPassTexture.Width / 2, sharedVariables.GraphicsManager.PreferredBackBufferHeight / 2 - levelPassTexture.Height / 2) */
                    new Rectangle(sharedVariables.GraphicsManager.PreferredBackBufferWidth / 2 - levelPassTexture.Width / 4, sharedVariables.GraphicsManager.PreferredBackBufferHeight/ 2 - levelPassTexture.Height / 4, levelPassTexture.Width / 2, levelPassTexture.Height / 2), Color.White);
            }

            if (sharedVariables.GamePlay.LevelFail)
            {
                spriteBatch.Draw(levelFailTexture,
                    new Rectangle(sharedVariables.GraphicsManager.PreferredBackBufferWidth / 2 - levelFailTexture.Width / 4, sharedVariables.GraphicsManager.PreferredBackBufferHeight / 2 - levelFailTexture.Height / 4, levelFailTexture.Width / 2, levelFailTexture.Height / 2), Color.White);
            }

            //pause
            if (paused)
            {
                spriteBatch.Draw(pausedTexture, pausedRectangle, Color.White);
                foreach (Control control in Controls)
                {
                    control.Draw(spriteBatch);
                    if(control.IsMouseOver)
                    {
                        spriteBatch.Draw(hoverTexture, buttonHoverRectangle, Color.White);
                    }
                }
            }

            spriteBatch.End();
        }
    }
}
