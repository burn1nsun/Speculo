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

        private Vector2 volumeLevelTxtPos;
        private string volumeLevelTxt;
        private bool showVolumeLevel;

        private Vector2 gameTimeTxtPos;
        private string gameTimeTxt;
        private Vector2 comboTxtPos;
        private string comboTxt;
        //private Vector2 levelTxtPos;
        //private string levelTxt;

        public SpriteFont hudFont; 

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
            //levelTxt = sharedVariables.GamePlay.Combo.ToString();

            Initialize();
        }

        public void Initialize()
        {   
            volumeLevelTxtPos = new Vector2(10, 25);
            comboTxtPos = new Vector2(10, sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y - (sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 100) * 10);
            gameTimeTxtPos = new Vector2(10, sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y - (sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 100) * 5);
            gameTimeTxt = TimeSpan.Zero.ToString();
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

            if(sharedVariables.GamePlay.IsPlaying)
            {
                comboTxt = sharedVariables.GamePlay.Combo.ToString() + "x";
                gameTimeTxt = sharedVariables.GamePlay.GameRuntime.ToString();
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
                if(sharedVariables.GamePlay.IsPlaying)
                {
                    spriteBatch.DrawString(hudFont, comboTxt, comboTxtPos, Color.White);
                    spriteBatch.DrawString(hudFont, gameTimeTxt, gameTimeTxtPos, Color.White);
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
