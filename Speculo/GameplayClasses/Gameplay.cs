using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speculo.GameplayClasses
{
    public class Gameplay
    {
        Utility.SharedVariables sharedVariables = Utility.SharedVariables.Instance;

        private Rectangle playArea;

        //public PlayArea() { get; set;}
        public Rectangle PlayArea
        {
            get { return this.playArea; }
            set { this.playArea = value; }
        }

        public Gameplay()
        {
            //playArea is the area the gameplay is happening, for example the character cannot move out of the play area. Playarea X is 16% of the screen.
            playArea = new Rectangle((sharedVariables.GraphicsManager.PreferredBackBufferWidth / 100) * 16, 0, sharedVariables.GraphicsManager.PreferredBackBufferWidth - (sharedVariables.GraphicsManager.PreferredBackBufferWidth / 100) * 32, sharedVariables.GraphicsManager.PreferredBackBufferHeight);
        }
    }
}
