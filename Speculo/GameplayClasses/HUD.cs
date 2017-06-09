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

        TimeSpan displayTime;
        TimeSpan lastDisplayed;

        //settings text
        private Vector2 volumeLevelTxtPos;
        private string volumeLevelTxt;
        private bool showVolumeLevel;

        //ingame text
        private Vector2 gameTimeTxtPos;
        private string gameTimeTxt;
        private Vector2 comboTxtPos;
        private string comboTxt;
        private Vector2 scoreTxtPos;
        private string scoreTxt;
        private Vector2 healthTxtPos;
        private string healthTxt;

        private Vector2 pauseTimePos;
        private string pauseTimeTxt;

        private Vector2 pausedTimePos;
        private string pausedTimeTxt;

        private Vector2 totalGameTimePos;
        private string totalGameTimeTxt;

        //health bar
        private Texture2D healthTexture;
        private Rectangle healthRectangle;

        //player ready

        private Vector2 playerReadyTxtPos;
        private string playerReadyTxt;

        //private Vector2 levelTxtPos;
        //private string levelTxt;


        public SpriteFont hudFont;
        private string biggestComboTxt;
        private Vector2 biggestComboTxtPos;

        public bool ShowHud { get; set; }



        public bool ShowVolumeLevel
        {
            get { return showVolumeLevel; }
            set { showVolumeLevel = value; }
        }

        public HUD()
        {
            showVolumeLevel = false;
            LoadContent(sharedVariables.Content);
            volumeLevelTxt = "SoundFx volume: " + String.Format("{0:0.0}", sharedVariables.SoundFxVolume) + "\r\n" + "Music volume: " + String.Format("{0:0.0}", sharedVariables.MusicVolume);

            comboTxt = "0x";
            biggestComboTxt = "0x";
            scoreTxt = "Score: 0";

            Initialize();
        }

        public void Initialize()
        {   
            volumeLevelTxtPos = new Vector2(10, 25);
            comboTxtPos = new Vector2(10, sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y - (sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 100) * 11);
            biggestComboTxtPos = new Vector2(10, sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y - (sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 100) * 14);
            scoreTxtPos = new Vector2(10, sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y - (sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 100) * 8);
            gameTimeTxtPos = new Vector2(10, sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y - (sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 100) * 5);
            gameTimeTxt = TimeSpan.Zero.ToString();

            pauseTimePos = new Vector2(10, sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y - (sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 100) * 3);
            pauseTimeTxt = sharedVariables.GamePlay.totalPauseTime.ToString();

            pausedTimePos = new Vector2(10, sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y - (sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 100) * 7);
            pausedTimeTxt = sharedVariables.GamePlay.pauseTime.ToString();

            totalGameTimePos = new Vector2(10, sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y - (sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 100) * 9);
            totalGameTimeTxt = sharedVariables.GamePlay.pauseTime.ToString();

            healthTxtPos = new Vector2(10, sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y - (sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 100) * 17);
            healthTxt = "Health: " + sharedVariables.GamePlay.Health.ToString();


            healthRectangle = new Rectangle(sharedVariables.GamePlay.PlayArea.X + 10,
            (int)(sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y - (sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 100) * 98),
            (int)sharedVariables.GamePlay.Health, 5);


            healthTexture = new Texture2D(sharedVariables.Graphics, 1, 1);
            healthTexture.SetData(new Color[] { new Color(0, 255, 0) });

            playerReadyTxt = "Press SPACE to ready up!";
            playerReadyTxtPos = new Vector2(sharedVariables.GraphicsManager.PreferredBackBufferWidth / 2 - ((int)hudFont.MeasureString(playerReadyTxt).X / 2), sharedVariables.GamePlay.PlayArea.Height / 3);
            

            //if(sharedVariables.Hud != null)
            //{
            //    sharedVariables.Hud.Initialize();
            //}
        }
        public void LoadContent(ContentManager Content)
        {
            
            hudFont = Content.Load<SpriteFont>("Fonts/Tahoma");
        }

        public void Update(GameTime gameTime)
        {
            if(showVolumeLevel)
            {
                if (gameTime.TotalGameTime > lastDisplayed + displayTime)
                {
                    showVolumeLevel = false;
                }
            }

            totalGameTimeTxt = gameTime.TotalGameTime.ToString() + " total gametime";

            if (sharedVariables.GamePlay.IsPlaying)
            {
                comboTxt = sharedVariables.GamePlay.Combo.ToString() + "x";
                biggestComboTxt = sharedVariables.GamePlay.BiggestCombo.ToString() + "x";
                scoreTxt = "Score: " + sharedVariables.GamePlay.Score.ToString();
                gameTimeTxt = "Level " + sharedVariables.GamePlay.CurrentLevel.ToString() + " : " + String.Format("{0:mm\\:ss}", sharedVariables.GamePlay.GameRuntime);
                healthTxt = "Health: " + sharedVariables.GamePlay.Health.ToString();
                healthRectangle.Width = (int)sharedVariables.GamePlay.Health;
                //pausedTimeTxt = sharedVariables.GamePlay.pauseTime.ToString() + " paused at";
                //pauseTimeTxt = sharedVariables.GamePlay.totalPauseTime.ToString() + " total pause time";
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (ShowHud)
            {   
                if(showVolumeLevel)
                {
                    spriteBatch.DrawString(hudFont, volumeLevelTxt, volumeLevelTxtPos, Color.White);
                }
                if (sharedVariables.GamePlay.IsPlaying)
                {
                    if(!sharedVariables.GamePlay.PlayerReady)
                    {
                        spriteBatch.DrawString(hudFont, playerReadyTxt, playerReadyTxtPos, Color.White);
                    }

                    spriteBatch.DrawString(hudFont, comboTxt, comboTxtPos, Color.White);
                    spriteBatch.DrawString(hudFont, biggestComboTxt, biggestComboTxtPos, Color.White);
                    spriteBatch.DrawString(hudFont, gameTimeTxt, gameTimeTxtPos, Color.White);
                    spriteBatch.DrawString(hudFont, scoreTxt, scoreTxtPos, Color.White);
                    spriteBatch.DrawString(hudFont, healthTxt, healthTxtPos, Color.White);
                    spriteBatch.Draw(healthTexture, healthRectangle, Color.White);
                    //sharedVariables
                    
                    //spriteBatch.DrawString(hudFont, pauseTimeTxt, pauseTimePos, Color.White);
                    //spriteBatch.DrawString(hudFont, pausedTimeTxt, pausedTimePos, Color.White);
                    //spriteBatch.DrawString(hudFont, totalGameTimeTxt, totalGameTimePos, Color.White);
                }
        }
        }

        public void volumeLevelChanged(GameTime gameTime)
        {
            displayTime = TimeSpan.FromSeconds(3);
            lastDisplayed = gameTime.TotalGameTime;

            volumeLevelTxt = "SoundFx volume: " + String.Format("{0: 0.0}", sharedVariables.SoundFxVolume) + "\r\n" + "Music volume: " + String.Format("{0:0.0}", sharedVariables.MusicVolume);

            showVolumeLevel = true;
        }
    }
}
