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

        public ContentManager Content { get; set; }
        public GraphicsDevice Graphics { get; set; }

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
            //throw new NotImplementedException();
        }
    }


}
