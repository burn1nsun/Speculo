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

            Initialize();
        }

        public void Initialize()
        {

            
            volumeLevelTxtPos = new Vector2(10, 25);
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
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (ShowHud)
            {   
                if(showVolumeLevel)
                {
                    spriteBatch.DrawString(hudFont, volumeLevelTxt, volumeLevelTxtPos, Color.White);
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
