﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
        public bool LevelComplete { get { return levelComplete; } set { levelComplete = value; } }

        public List<Enemy> enemyList;
        public List<Enemy> enemiesToRemove;

        private bool levelComplete;

        private float playAreaSector; //1 sector is 5% of playarea

        private SoundEffect levelCompleteSound;

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
            levelCompleteSound = sharedVariables.Content.Load<SoundEffect>("Sound/Gameplay/sectionpass");

            initialize();
        }

        public void initialize()
        {
            CharacterClass = new Character();

            enemyList = new List<Enemy>();
            enemiesToRemove = new List<Enemy>();

            levelComplete = false;

            gameStartTime = sharedVariables.gameTime.TotalGameTime;

            addEnemies();
            playArea = new Rectangle(((int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 100) * 16, 0, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X - ((int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 100) * 32, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y);
            playAreaSector = playArea.Width / 100 * 5;
        }
        public void Update(GameTime gameTime)
        {
            gameRuntime = gameTime.TotalGameTime - gameStartTime;

            foreach (Enemy enemy in enemyList.ToList()) //tolist because otherwise otherwise Collection was modified; enumeration operation may not execute exception
            {
                if (!enemy.IsDead)
                {
                    enemy.Update(gameTime);
                }
                else
                {
                    enemiesToRemove.Add(enemy);
                    removeEnemies();
                }  
            }

        }

        public void levelCompleted()
        {
            levelComplete = true;
        }

        public void addEnemies()
        {
            enemyList.Clear();

            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2000), playAreaSector * 1));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2200), playAreaSector * 3));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2300), playAreaSector * 2));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2400), playAreaSector * 1));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2600), playAreaSector * 2));

            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(3000), playAreaSector * 3));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(3200), playAreaSector * 1));

            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(4000), playAreaSector * 5));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(4200), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(4300), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(4400), playAreaSector * 5));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(4600), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5000), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5200), playAreaSector * 5));

            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5300), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5400), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5450), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5500), playAreaSector * 9));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5600), playAreaSector * 9));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5700), playAreaSector * 9));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5800), playAreaSector * 9));
        }

        void removeEnemies()
        {
            foreach (Enemy enemy in enemiesToRemove)
            {
                enemyList.Remove(enemy);
            }
            enemiesToRemove.Clear();
            if (enemyList.Count() == 0)
            {
                levelCompleted();
                levelCompleteSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
            }
        }

    }
}
