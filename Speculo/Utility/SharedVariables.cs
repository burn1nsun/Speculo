using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speculo.Utility
{
    public sealed class SharedVariables
    {
        private static SharedVariables instance = null;
        private static readonly object padlock = new object();

        private List<Vector2> screenSizes;

        
        public ContentManager Content { get; set; }
        public GraphicsDevice Graphics { get; set; }
        public GraphicsDeviceManager GraphicsManager { get; set; }
        public List<Vector2> ScreenSizes { get { return screenSizes; } set { screenSizes = value; } }
        public int ScreenSizeIndex { get; set; }

        public float SoundFxVolume { get; set; }
        public float MusicVolume { get; set; }

        public GameplayClasses.HUD Hud { get; set; }
        public CharacterClasses.Character CharacterClass { get; set; }
        public GameplayClasses.Gameplay GamePlay { get; set; }
        

        static bool called = false;
        SharedVariables()
        {


        }

        public static SharedVariables Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new SharedVariables();
                    }
                    return instance;
                }
            }
        }

        internal void initVariables()
        {
            if (!called && Graphics != null && GraphicsManager != null && Content != null)
            {
                ScreenSizeIndex = 4;
                screenSizes = new List<Vector2>();
                screenSizes.InsertRange(ScreenSizes.Count, new Vector2[] { new Vector2(800, 600), new Vector2(1024, 768), new Vector2(1280, 960), new Vector2(1366, 768), new Vector2(1600, 900), new Vector2(1680, 1080), new Vector2(1920, 1080)});

                SoundFxVolume = 1.0f;
                MusicVolume = 1.0f;

                Hud = new GameplayClasses.HUD();
                GamePlay = new GameplayClasses.Gameplay();

                CharacterClass = new CharacterClasses.Character();
                called = true;
            }
            
        }
    }


}
