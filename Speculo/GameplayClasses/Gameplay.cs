using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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

        protected MouseState presentMouse;
        protected MouseState pastMouse;

        public Character CharacterClass { get; set; }
        public bool IsPlaying { get { return isPlaying; } set { isPlaying = value; } }
        public bool LevelFail { get { return levelFail; } set { levelFail = value; } }

        public List<Enemy> enemyList;
        public List<Enemy> enemiesToRemove;

        private int score;

        private bool levelComplete;

        private float playAreaSector; //1 sector is 5% of playarea

        private float health;
        Random random = new Random();

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
        private bool playerReady;
        private SoundEffect readySound;
        private Song levelSong;
        private Texture2D characterBorder;

        public Texture2D PlayAreaBorder
        {
            get { return playAreaBorder; }
            set { playAreaBorder = value; }
        }

        public Texture2D CharacterBorder
        {
            get { return characterBorder; }
            set { characterBorder = value; }
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

        public bool PlayerReady
        {
            get { return playerReady; }
            set { this.playerReady = value; }
        }

        //public PlayArea() { get; set;}
        public Rectangle PlayArea
        {
            get { return this.playArea; }
            set { this.playArea = value; }
        }

        public int CurrentLevel { get { return currentLevel; } set { currentLevel = value; } }

        public float Health { get { return health; } set { health = value; } }

        public bool IsLeftClicked
        {
            get { return (presentMouse.LeftButton == ButtonState.Released && pastMouse.LeftButton == ButtonState.Pressed); }
        }

        public Gameplay()
        {
            //playArea is the area the gameplay is happening, for example the character cannot move out of the play area. Playarea X is 16% of the screen.
            levelCompleteSound = sharedVariables.Content.Load<SoundEffect>("Sound/Gameplay/sectionpass");
            levelFailSound = sharedVariables.Content.Load<SoundEffect>("Sound/Gameplay/sectionfail");
            readySound = sharedVariables.Content.Load<SoundEffect>("Sound/Gameplay/readysound");

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

            characterBorder = new Texture2D(sharedVariables.Graphics, CharacterClass.Rectangle.Width, CharacterClass.Rectangle.Height);
            characterBorder.CreateBorder(1, Color.Red);

            enemyList = new List<Enemy>();
            enemiesToRemove = new List<Enemy>();

            combo = 0;
            score = 0;
            
            currentLevel = 1;
            levelComplete = false;
            playerReady = false;

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
            playerReady = false;

            totalPauseTime = TimeSpan.Zero;
            pauseTime = TimeSpan.Zero;
            gameRuntime = TimeSpan.Zero;
            lastPauseTime = TimeSpan.Zero;
            GameStartTime = TimeSpan.Zero;

            if (sharedVariables.Hud != null)
            {
                sharedVariables.Hud.Initialize();
            }

            addEnemies();

        }
        public void Update(GameTime gameTime)
        {
            if(playerReady)
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
            else
            {
                promptPlayerReady(gameTime);
            }

            pastMouse = presentMouse;
            presentMouse = Mouse.GetState();

            
            if (IsLeftClicked)
            {
                Console.WriteLine("enemyList.Add(new Enemy(TimeSpan.FromSeconds(" + (gameRuntime - TimeSpan.FromSeconds(1.100) - gameStartTime) + "), playAreaSector * " + random.Next(3, 8) + ");");
            }



        }

        public void promptPlayerReady(GameTime gameTime)
        {
            if(Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                playerReady = true;
                gameStartTime = gameTime.TotalGameTime;
                readySound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                level1Song();
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
            MediaPlayer.Stop();
        }

        public void addEnemies()
        {
            enemyList.Clear();
            level1();
        }

        public void level1Song()
        {
            currentLevel = 1;
            levelSong = sharedVariables.Content.Load<Song>("Sound/Gameplay/Songs/crystallized");

            MediaPlayer.Volume = sharedVariables.MusicVolume;
            MediaPlayer.Play(levelSong, TimeSpan.FromSeconds(27));
        }

        //public void writeLevel()
        //{
        //    MouseState presentMouse = new MouseState();
        //    if(presentMouse.LeftButton == ButtonState.Pressed)
        //    {
        //        Console.WriteLine("enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(" + (gameRuntime - TimeSpan.FromMilliseconds(1100)) + "), playAreaSector * 8))");
        //    }
            
        //}

        public void level1()
        {


            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 0.2133202), playAreaSector * 1));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 0.3799852), playAreaSector * 3));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 0.5466502), playAreaSector * 3));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 0.7399816), playAreaSector * 1));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 0.8999800), playAreaSector * 3));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 1.0799782), playAreaSector * 3));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 1.2533098), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 1.4066416), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 1.5999730), playAreaSector * 8));

            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 2.6066296), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 3.1932904), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 03.3732886), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 04.0399486), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 04.5599434), playAreaSector * 12));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 04.7399416), playAreaSector * 13));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 04.8999400), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 05.0799382), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 05.4332680), playAreaSector * 1));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 05.9399296), playAreaSector * 5));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 06.0999280), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 06.2732596), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 06.4532578), playAreaSector * 3));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 06.7932544), playAreaSector * 4));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 07.2399166), playAreaSector * 5));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 07.4465812), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 07.7999110), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 08.1599074), playAreaSector * 11));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 08.6799022), playAreaSector * 12));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 08.8532338), playAreaSector * 11));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 09.0132322), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 09.1998970), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 09.5465602), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 10.0265554), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 10.1998870), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 10.3798852), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 10.5598834), playAreaSector * 1));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 10.7198818), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 10.8998800), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 11.0532118), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 11.2465432), playAreaSector * 14));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 11.6265394), playAreaSector * 9));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 11.9265364), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 12.1065346), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 12.2731996), playAreaSector * 5));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 12.4398646), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 12.6331960), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 12.7998610), playAreaSector * 5));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 12.9065266), playAreaSector * 4));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 12.9598594), playAreaSector * 3));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 13.1531908), playAreaSector * 2));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 13.3465222), playAreaSector * 2));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 13.6998520), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 14.1798472), playAreaSector * 9));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 14.3665120), playAreaSector * 11));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 14.5265104), playAreaSector * 14));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 14.7198418), playAreaSector * 14));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 15.0531718), playAreaSector * 15));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 15.5998330), playAreaSector * 15));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 15.7664980), playAreaSector * 17));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 15.9464962), playAreaSector * 15));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 16.1064946), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 16.2664930), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 16.4464912), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 16.9664860), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 17.1331510), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 17.3064826), playAreaSector * 5));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 17.4731476), playAreaSector * 4));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 17.6398126), playAreaSector * 4));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 17.8064776), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 17.9864758), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 18.1731406), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 18.3331390), playAreaSector * 9));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 18.5131372), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 18.6864688), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 18.8598004), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 19.0264654), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 19.2064636), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 19.6597924), playAreaSector * 5));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 19.8464572), playAreaSector * 4));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 20.0331220), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 20.2131202), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 20.3797852), playAreaSector * 12));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 20.5464502), playAreaSector * 12));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 20.7197818), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 20.8931134), playAreaSector * 9));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 21.0731116), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 21.2397766), playAreaSector * 11));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 21.4131082), playAreaSector * 12));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 21.5997730), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 21.7664380), playAreaSector * 11));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 21.9331030), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 22.0931014), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 22.2730996), playAreaSector * 5));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 22.4264314), playAreaSector * 4));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 22.6330960), playAreaSector * 5));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.0197588), playAreaSector * 4));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.0930914), playAreaSector * 7));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.1930904), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.2597564), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.3530888), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.4130882), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.5064206), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.5597534), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.6664190), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.7264184), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.8130842), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.8730836), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 23.9730826), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 24.0330820), playAreaSector * 3));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 25.0530718), playAreaSector * 5));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 25.6663990), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 25.7397316), playAreaSector * 6));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 25.9063966), playAreaSector * 8));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 26.0330620), playAreaSector * 13));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 26.3797252), playAreaSector * 13));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 26.4597244), playAreaSector * 13));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 26.5197238), playAreaSector * 13));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 26.7663880), playAreaSector * 12));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 26.8663870), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 26.9197198), playAreaSector * 10));
            enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.6 + 27.0863848), playAreaSector * 8));

            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(2.5399916), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(2.3733266), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(2.1999950), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(1.9866638), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(1.8199988), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(1.6533338), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(1.4800022), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(1.3200038), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(1.0666730), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.7800092), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.2600144), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(0.0533498), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(00.3066466), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(00.6399766), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(01.1199718), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(01.2999700), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(01.4733016), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(01.6599664), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(01.9666300), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(02.4732916), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(02.6532898), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(02.8466212), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(03.0066196), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(03.1732846), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(03.3599494), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(03.7732786), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(03.9932764), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(04.2532738), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(04.4732716), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(04.7732686), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(05.2599304), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(05.4399286), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(05.5932604), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(05.7599254), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(06.1265884), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(06.5799172), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(06.7732486), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(06.9599134), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(07.1199118), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(07.2999100), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(07.4599084), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(07.6332400), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(07.8065716), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(07.9799032), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(08.1599014), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(08.3332330), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(08.5198978), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(08.6865628), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(08.8532278), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(09.0065596), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(09.1932244), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(09.2865568), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(09.3398896), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(09.4532218), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(09.5132212), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(09.6398866), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(09.7865518), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(09.9732166), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(10.2532138), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(10.7198758), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(10.8798742), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(11.0665390), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(11.2465372), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(11.4198688), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(11.6265334), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(12.1731946), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(12.2398606), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(12.3598594), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(12.5531908), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(12.7131892), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(13.0131862), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(13.5131812), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(13.6331800), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(13.6998460), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(13.8665110), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(14.0265094), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(14.1931744), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(14.3731726), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(14.5598374), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(14.7331690), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(14.8998340), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(15.0864988), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(15.4064956), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(15.5731606), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(15.7664920), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(15.9398236), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(16.0998220), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(16.2864868), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(16.4731516), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(16.6464832), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(16.7998150), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(16.9731466), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(17.1664780), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(17.3398096), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(17.4931414), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(17.6598064), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(17.8064716), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(18.0064696), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(18.1798012), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(18.2731336), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(18.3331330), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(18.4797982), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(18.5797972), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(18.6464632), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(18.7331290), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(18.7931284), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(18.8931274), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(18.9531268), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.0464592), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.1064586), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.1931244), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.2397906), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.3597894), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.4131222), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.5197878), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.5664540), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.6664530), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.7397856), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.8264514), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.8997840), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(19.9931164), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(20.0397826), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(20.1531148), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(20.2264474), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(20.3197798), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(20.3797792), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(20.5197778), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(20.6597764), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(21.5797672), playAreaSector * 3));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(22.5264244), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(23.0397526), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(23.2464172), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(23.3330830), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(23.3797492), playAreaSector * 7));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(23.4997480), playAreaSector * 4));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(23.6797462), playAreaSector * 5));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(23.8464112), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(24.0130762), playAreaSector * 6));
            //enemyList.Add(new Enemy(TimeSpan.FromSeconds(24.2064076), playAreaSector * 3));

        }

        internal void pause(GameTime gameTime)
        {
            IsPlaying = false;
            MediaPlayer.Pause();
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
            MediaPlayer.Resume();
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
    public static class Utilities
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
