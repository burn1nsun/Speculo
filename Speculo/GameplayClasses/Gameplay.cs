using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Speculo.CharacterClasses;
using Speculo.UserControls;
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

        private TimeSpan gameRuntime;
        private TimeSpan gameStartTime;
        private Rectangle playArea;

        public Character CharacterClass { get; set; }

        public List<Enemy> enemyList;

        public TimeSpan GameStartTime
        {
            get { return gameStartTime;  }
            set { this.gameStartTime = value; }
        }

        //public PlayArea() { get; set;}
        public Rectangle PlayArea
        {
            get { return this.playArea; }
            set { this.playArea = value; }
        }

        public Gameplay()
        {
            //playArea is the area the gameplay is happening, for example the character cannot move out of the play area. Playarea X is 16% of the screen.
            initialize();
        }

        public void initialize()
        {
            CharacterClass = new Character();

            enemyList = new List<Enemy>();
            gameStartTime = sharedVariables.gameTime.TotalGameTime;

            addEnemies();
            playArea = new Rectangle(((int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 100) * 16, 0, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X - ((int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 100) * 32, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y);
        }
        public void Update(GameTime gameTime)
        {
            gameRuntime = gameTime.TotalGameTime - gameStartTime;

            foreach(Enemy enemy in enemyList)
            {
                enemy.Update(gameTime);
            }
        }

        public void addEnemies()
        {
            enemyList.Clear();
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(2), 200));
        }

    }
}
