using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Speculo.GameplayClasses;
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


       
        public ContentManager Content { get; set; }
        public GraphicsDevice Graphics { get; set; }
        public GraphicsDeviceManager GraphicsManager { get; set; }
        public Vector2 screenSize { get; set; }
        public HUD Hud { get; set; }

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
            
            Hud = new HUD();
        }
    }


}
