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


        private Vector2 boundsPosition;
        private Rectangle boundsRectangle;
        private Texture2D boundsTexture;

        TimeSpan displayTime;
        TimeSpan lastDisplayed;

        private Vector2 volumeLevelTxtPos;
        private string volumeLevelTxt;
        private bool showVolumeLevel;


        public SpriteFont hudFont; 

        public bool ShowHud { get; set; }

        public Rectangle BoundsRectangle
        {
            get { return boundsRectangle; }
            set { boundsRectangle = value; }
        }

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
            boundsPosition = new Vector2(0, 0);
            boundsRectangle = new Rectangle((int)boundsPosition.X, (int)boundsPosition.Y, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y);
            
            volumeLevelTxtPos = new Vector2(10, 25);
        }
        public void LoadContent(ContentManager Content)
        {
            boundsTexture = Content.Load<Texture2D>("Textures/bounds");
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
                spriteBatch.Draw(boundsTexture, boundsRectangle, Color.White);
                
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
