using Microsoft.Xna.Framework;
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
        public bool IsPlaying { get { return isPlaying; } set { isPlaying = value; } }
        public bool LevelFail { get { return levelFail; } set { levelFail = value; } }

        public List<Enemy> enemyList;
        public List<Enemy> enemiesToRemove;

        private int score;

        private bool levelComplete;

        private float playAreaSector; //1 sector is 5% of playarea

        private float health;

        private SoundEffect levelCompleteSound;
        private bool isPlaying;
        private int currentLevel;
        private int combo;

        public TimeSpan totalPauseTime;
        public TimeSpan pauseTime;
        public TimeSpan lastPauseTime;
        public static Texture2D playAreaBorder;
        private bool levelFail;
        private SoundEffect levelFailSound;
        private bool died;

        public Texture2D PlayAreaBorder
        {
            get { return playAreaBorder; }
            set { playAreaBorder = value; }
        }

        public int Combo
        {
            get { return combo; }
            set { this.combo = value; }
        }

        public int Score
        {
            get { return score; }
            set { this.score = value; }
        }

        public TimeSpan GameStartTime
        {
            get { return gameStartTime;  }
            set { this.gameStartTime = value; }
        }

        public TimeSpan GameRuntime
        {
            get { return gameRuntime; }
            set { this.gameRuntime = value; }
        }

        public bool LevelComplete
        {
            get { return levelComplete; }
            set { this.levelComplete = value; }
        }

        //public PlayArea() { get; set;}
        public Rectangle PlayArea
        {
            get { return this.playArea; }
            set { this.playArea = value; }
        }

        public int CurrentLevel { get { return currentLevel; } set { currentLevel = value; } }

        public float Health { get { return health; } set { health = value; } }

        public Gameplay()
        {
            //playArea is the area the gameplay is happening, for example the character cannot move out of the play area. Playarea X is 16% of the screen.
            levelCompleteSound = sharedVariables.Content.Load<SoundEffect>("Sound/Gameplay/sectionpass");
            levelFailSound = sharedVariables.Content.Load<SoundEffect>("Sound/Gameplay/sectionfail");

            health = 400;
            //initialize();//
            playArea = new Rectangle(
                ((int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 100) * 16,
                0,
                (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X - ((int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 100) * 32,
                (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y
                );
            playAreaSector = playArea.Width / 100 * 5;

            playAreaBorder = new Texture2D(sharedVariables.Graphics, playArea.Width, playArea.Height);
            playAreaBorder.CreateBorder(1, Color.Red);

            CharacterClass = new Character();

            enemyList = new List<Enemy>();
            enemiesToRemove = new List<Enemy>();

            combo = 0;
            score = 0;
            levelComplete = false;
            currentLevel = 1;

            totalPauseTime = TimeSpan.Zero;
            pauseTime = TimeSpan.Zero;
            gameRuntime = TimeSpan.Zero;
            lastPauseTime = TimeSpan.Zero;

            addEnemies();
        }

        public void initialize(GameTime gameTime)
        {
            playArea = new Rectangle(((int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 100) * 16, 0, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X - ((int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 100) * 32, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y);
            playAreaSector = playArea.Width / 100 * 5;

            playAreaBorder = new Texture2D(sharedVariables.Graphics, playArea.Width, playArea.Height);
            playAreaBorder.CreateBorder(1, Color.Red);

            CharacterClass = new Character();

            enemyList = new List<Enemy>();
            enemiesToRemove = new List<Enemy>();

            combo = 0;
            score = 0;
            health = 400;
            levelComplete = false;
            levelFail = false;
            died = false;
            currentLevel = 1;

            totalPauseTime = TimeSpan.Zero;
            pauseTime = TimeSpan.Zero;
            gameRuntime = TimeSpan.Zero;
            lastPauseTime = TimeSpan.Zero;
            GameStartTime = gameTime.TotalGameTime;

            if (sharedVariables.Hud != null)
            {
                sharedVariables.Hud.Initialize();
            }

            addEnemies();

        }
        public void Update(GameTime gameTime)
        {
            if(!levelComplete)
            {
                gameRuntime = gameTime.TotalGameTime - gameStartTime - totalPauseTime;
            }

            if(!died)
            {
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
                if (enemyList.Count != 0)
                {
                    if (enemyList[0].EnemySent)
                    {
                        health = MathHelper.Clamp(health - 0.1f, 0, 400);
                    }
                }
            }

            if(health <= 0 && !died)
            {
                die();
            }
        }

        public void die()
        {
            levelFail = true;
            isPlaying = false;
            died = true;
            levelFailSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
        }
        public void killedEnemy()
        {
            addCombo();
            addScore();
        }

        public void addScore()
        {
            if(combo > 0)
            {
                score += combo * 100;
            }
            else
            {
                combo += 100;
            }
        }

        public void levelCompleted()
        {
            levelComplete = true;
        }

        public void addEnemies()
        {
            enemyList.Clear();
            level1();
        }

        public void level1()
        {
            currentLevel = 1;

            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2000), playAreaSector * 1));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2200), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2600), playAreaSector * 15));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2800), playAreaSector * 18));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(3000), playAreaSector * 10));

            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(3300), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(3600), playAreaSector * 14));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(3800), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(4200), playAreaSector * 14));

            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5200), playAreaSector * 1));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5400), playAreaSector * 4));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5450), playAreaSector * 4));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5500), playAreaSector * 4));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5550), playAreaSector * 4));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5600), playAreaSector * 4));


            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5750), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5800), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5850), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5900), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5950), playAreaSector * 6));
        }

        internal void pause(GameTime gameTime)
        {
            IsPlaying = false;

            pauseTime = gameTime.TotalGameTime;
        }

        void level2()
        {
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

            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5300), playAreaSector * 3));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5400), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5450), playAreaSector * 5));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5500), playAreaSector * 3));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5600), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5700), playAreaSector * 3));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5800), playAreaSector * 10));
        }

        internal void unpause(GameTime gameTime)
        {
            lastPauseTime = totalPauseTime;
            IsPlaying = true;
        }

        public void addCombo()
        {
            combo++;
            health = MathHelper.Clamp(health + 20, 0, 400);
        }

        public void breakCombo()
        {
            combo = 0;
            health = MathHelper.Clamp(health - 100, 0, 400);
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
    static class Utilities
    {
        public static void CreateBorder(this Texture2D texture, int borderWidth, Color borderColor)
        {

            Color[] colors = new Color[texture.Width * texture.Height];

            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    bool colored = false;
                    for (int i = 0; i <= borderWidth; i++)
                    {
                        if (x == i || y == i || x == texture.Width - 1 - i || y == texture.Height - 1 - i)
                        {
                            colors[x + y * texture.Width] = borderColor;
                            colored = true;
                            break;
                        }
                    }

                    if (colored == false)
                        colors[x + y * texture.Width] = Color.Transparent;
                }
            }

            texture.SetData(colors);
        }
    }
}
