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
            MediaPlayer.Play(levelSong /*, TimeSpan.FromSeconds(27)*/);
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

            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 9007), 147));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 11765), 268));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 13144), 195));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 13834), 140));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 14524), 339));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 17282), 410));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 18662), 264));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 20041), 104));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 20213), 167));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 20558), 85));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 20903), 152));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 21248), 254));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 21334), 255));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 21420), 256));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 21593), 327));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 21679), 326));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 21765), 325));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 21938), 236));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 22024), 237));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 22110), 238));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 22283), 309));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 22369), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 22455), 307));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 22800), 413));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 22972), 350));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 23317), 432));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 23662), 365));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 24007), 251));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 24093), 249));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 24179), 247));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 24351), 159));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 24524), 136));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 24869), 139));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 25041), 34));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 25213), 18));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 25558), 136));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 25903), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 26420), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 26593), 179));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 26938), 216));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 27110), 216));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 27455), 299));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 27713), 304));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 27800), 304));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 27972), 391));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 28231), 499));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 28317), 504));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 28489), 496));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 28662), 393));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 28921), 413));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 29007), 410));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 29093), 407));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 29179), 404));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 29351), 271));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 29610), 374));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 29696), 379));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 29869), 393));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 30041), 393));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 30213), 251));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 30472), 279));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 30731), 135));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 30903), 53));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 31075), 69));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 31248), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 31420), 226));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 31679), 207));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 31765), 234));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 31851), 264));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 31938), 295));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 32110), 512));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 32455), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 32627), 226));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 32800), 226));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 32972), 295));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 33058), 297));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 33144), 307));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 33231), 328));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 33317), 354));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 33489), 133));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 33834), 56));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 34007), 225));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 34179), 44));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 34438), 171));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 34524), 158));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 34610), 157));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 34696), 163));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 34869), 371));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 35213), 357));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 35386), 291));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 35558), 432));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 35731), 443));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 35817), 443));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 36075), 464));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 36248), 321));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 36420), 512));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 36593), 371));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 36938), 81));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 37196), 78));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 37282), 106));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 37369), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 37455), 168));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 37627), 65));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 37972), 174));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 38144), 186));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 38317), 186));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 38489), 259));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 38575), 258));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 38662), 257));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 38748), 255));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 38834), 254));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 39007), 292));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 39179), 168));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 39351), 391));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 39696), 444));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 39869), 313));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 39955), 341));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 40041), 372));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 40127), 402));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 40213), 431));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 40386), 501));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 40731), 310));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 40903), 348));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 41075), 348));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 41248), 228));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 41420), 266));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 41593), 236));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 41765), 146));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 41938), 56));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 42024), 104));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 42110), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 42455), 66));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 42627), 162));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 42713), 190));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 42800), 221));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 42886), 251));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 42972), 280));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 43144), 468));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 43489), 228));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 43662), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 43834), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 44007), 32));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 44093), 61));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 44179), 91));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 44266), 122));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 44352), 150));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 44524), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 44869), 309));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 45041), 420));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 45213), 229));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 45471), 358));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 45557), 357));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 45644), 361));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 45730), 373));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 45903), 408));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 46248), 419));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 46420), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 46593), 494));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 46765), 300));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 46851), 300));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 47110), 319));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 47282), 216));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 47455), 419));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 47627), 165));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 47972), 165));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 48144), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 48231), 133));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 48317), 128));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 48403), 117));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 48489), 95));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 48662), 209));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 49007), 286));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 49179), 341));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 49351), 165));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 49524), 398));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 49610), 399));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 49696), 400));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 49782), 402));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 49869), 403));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 50041), 414));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 50386), 195));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 50472), 191));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 50558), 188));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 50644), 185));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 50731), 183));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 50817), 182));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 50903), 182));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 50989), 183));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 51075), 184));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 51162), 156));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 51248), 111));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 51334), 75));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 51420), 67));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 51507), 86));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 51593), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 51679), 167));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 51765), 249));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 52455), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 53144), 141));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 53834), 90));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 54524), 43));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 55041), 93));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 55558), 149));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 56248), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 56420), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 56593), 277));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 56765), 361));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 56938), 367));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 57282), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 57799), 361));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 58316), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 59007), 240));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 59179), 464));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 59351), 201));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 59524), 401));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 59696), 272));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 60041), 228));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 60558), 278));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 61075), 334));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 61765), 394));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 62110), 476));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 62282), 267));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 62455), 279));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 62800), 195));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 63144), 382));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 63317), 139));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 63489), 332));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 63662), 190));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 63834), 361));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 64179), 437));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 64869), 381));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 65213), 216));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 65386), 498));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 65558), 274));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 66075), 267));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 66593), 285));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 67282), 327));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 67627), 402));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 67972), 236));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 68144), 477));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 68317), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 68834), 304));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 69352), 285));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 70041), 263));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 70386), 277));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 70558), 330));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 70903), 495));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 71075), 311));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 71592), 303));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 72110), 330));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 72627), 413));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 72800), 338));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 73144), 212));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 73317), 37));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 73489), 37));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 73833), 162));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 74007), 235));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 74179), 235));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 74523), 110));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 74696), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 74869), 285));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 75213), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 75386), 430));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 75558), 442));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 75731), 384));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 75903), 396));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 76075), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 76248), 354));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 76420), 366));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 76592), 381));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 76765), 327));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 76937), 339));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 77109), 354));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 77282), 293));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 77454), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 77626), 320));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 77800), 320));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 77972), 302));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 78145), 168));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 78317), 156));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 78490), 214));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 78662), 202));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 78834), 187));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 79007), 244));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 79179), 232));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 79351), 217));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 79524), 271));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 79696), 259));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 79868), 244));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 80041), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 80213), 293));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 80385), 278));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 80559), 278));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 80731), 164));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 80903), 140));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 81075), 91));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 81248), 60));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 81420), 269));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 81593), 293));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 81764), 342));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 81937), 373));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 82110), 455));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 82282), 456));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 82455), 198));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 82627), 192));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 82800), 470));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 82972), 473));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 83144), 172));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 83316), 161));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 83489), 431));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 83575), 409));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 83662), 383));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 83748), 357));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 83834), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 83920), 294));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 84007), 268));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 84093), 242));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 84179), 196));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 84524), 161));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 84869), 116));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 85731), 224));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 85903), 268));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 85989), 267));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 86075), 264));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 86162), 263));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 86248), 283));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 86420), 352));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 86593), 195));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 86765), 101));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 86851), 108));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 86938), 116));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 87110), 352));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 87282), 85));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 87455), 159));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 87627), 189));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 87800), 61));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 87972), 77));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 88144), 229));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 88231), 266));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 88317), 298));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 88403), 324));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 88489), 340));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 88662), 376));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 88834), 353));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 89007), 373));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 89179), 424));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 89351), 266));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 89524), 425));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 89696), 222));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 89869), 449));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 90041), 203));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 90213), 216));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 90386), 317));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 90558), 79));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 90731), 325));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 90817), 339));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 90903), 328));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 90989), 289));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 91075), 241));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 91162), 267));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 91248), 289));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 91334), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 91420), 315));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 91765), 304));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 91938), 451));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 92110), 238));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 92282), 430));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 92455), 433));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 92541), 433));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 92627), 433));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 92800), 304));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 92972), 126));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 93144), 315));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 93317), 126));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 93489), 198));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 93662), 145));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 93748), 98));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 93834), 61));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 93920), 47));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 94007), 54));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 94179), 180));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 94351), 126));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 94524), 324));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 94696), 61));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 94869), 323));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 95127), 255));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 95213), 264));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 95386), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 95558), 218));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 95731), 271));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 95903), 415));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 96075), 410));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 96248), 114));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 96507), 285));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 96593), 302));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 96765), 169));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 96938), 187));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 97282), 358));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 97455), 415));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 97627), 363));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 97800), 245));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 97886), 245));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 97972), 245));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 98144), 423));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 98317), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 98489), 241));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 98662), 219));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 98834), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 98920), 420));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 99007), 419));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 99179), 186));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 99351), 424));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 99524), 329));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 99696), 288));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 99869), 322));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 100041), 500));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 100213), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 100386), 492));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 100558), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 100731), 418));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 100903), 225));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 101075), 499));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 101248), 144));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 101420), 383));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 101765), 75));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 101851), 68));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 101938), 77));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 102024), 100));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 102110), 134));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 102196), 114));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 102282), 100));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 102369), 94));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 102455), 95));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 102800), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 103144), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 103317), 221));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 103489), 250));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 103662), 77));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 103834), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 104007), 163));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 104093), 163));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 104179), 163));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 104351), 149));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 104524), 325));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 104696), 72));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 104869), 327));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 105041), 335));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 105213), 358));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 105558), 360));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 105731), 454));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 105903), 187));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 106248), 358));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 106420), 368));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 106593), 267));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 106765), 454));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 106938), 150));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 107110), 348));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 107282), 311));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 107455), 168));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 107627), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 108317), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 109007), 176));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 109179), 279));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 109265), 279));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 109351), 279));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 109524), 194));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 109696), 380));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 109782), 380));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 109868), 380));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 110041), 209));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 110213), 482));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 110386), 249));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 110558), 358));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 110903), 227));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 111075), 49));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 111420), 180));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 111765), 11));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 111938), 19));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 112110), 19));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 112455), 28));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 112800), 84));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 112972), 183));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 113144), 175));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 113317), 196));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 113489), 278));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 113662), 40));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 113834), 205));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 114524), 315));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 114696), 132));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 114782), 132));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 114869), 132));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 115041), 264));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 115213), 38));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 115299), 38));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 115386), 38));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 115558), 307));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 115731), 81));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 115903), 351));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 116075), 351));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 116420), 339));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 116593), 427));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 116938), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 117282), 206));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 117455), 206));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 117627), 201));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 117800), 123));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 117972), 350));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 118144), 346));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 118317), 346));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 118489), 115));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 118661), 119));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 118834), 119));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 119007), 399));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 119093), 416));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 119179), 379));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 119265), 310));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 119351), 281));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 120041), 229));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 120213), 466));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 120386), 156));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 120558), 416));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 120730), 179));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 120903), 489));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 121075), 135));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 121248), 372));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 121420), 95));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 121593), 37));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 121851), 228));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 122110), 45));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 122455), 95));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 122800), 110));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 122972), 263));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 123144), 91));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 123316), 244));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 123489), 69));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 123834), 149));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 124007), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 124093), 338));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 124179), 368));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 124351), 176));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 124524), 435));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 124696), 115));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 124869), 424));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 125558), 508));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 125731), 270));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 125903), 477));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 126075), 156));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 126248), 394));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 126420), 187));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 126593), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 126765), 204));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 126938), 481));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 127282), 455));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 127627), 322));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 127800), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 127972), 116));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 128144), 128));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 128317), 341));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 128489), 370));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 128662), 95));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 128834), 73));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 129007), 188));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 129093), 202));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 129179), 217));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 129265), 232));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 129351), 247));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 129438), 262));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 129524), 277));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 129610), 292));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 129696), 307));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 129782), 321));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 129869), 335));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 130041), 191));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 130213), 353));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 130386), 219));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 146938), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 147627), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 148317), 272));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 148489), 273));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 148662), 274));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 148834), 275));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 149007), 350));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 149179), 348));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 149351), 346));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 149524), 344));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 149696), 281));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 149782), 288));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 149869), 295));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 149955), 302));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 150041), 309));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 150127), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 150213), 323));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 150300), 330));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 150386), 336));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 150472), 307));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 150558), 264));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 150644), 226));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 150731), 196));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 150817), 153));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 150903), 111));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 150989), 82));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 151075), 59));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 151765), 123));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 152110), 80));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 152455), 58));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 153144), 202));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 153834), 154));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 154351), 267));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 154869), 380));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 155558), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 155731), 280));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 155903), 192));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 156076), 168));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 156248), 105));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 156593), 105));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 157110), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 157627), 170));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 158317), 167));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 158662), 173));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 158834), 237));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 159007), 336));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 159351), 318));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 159869), 393));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 160386), 235));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 161075), 280));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 161248), 82));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 161334), 53));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 161420), 28));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 161507), 23));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 161593), 27));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 161679), 37));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 161765), 89));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 161852), 94));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 161938), 90));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 162024), 80));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 162110), 15));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 162282), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 162455), 207));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 162800), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 162972), 133));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 163144), 292));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 163489), 381));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 163834), 293));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 164179), 399));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 164524), 311));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 164869), 194));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 165214), 106));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 165559), 212));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 165904), 124));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 166248), 400));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 166593), 312));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 166938), 418));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 167283), 330));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 167628), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 167973), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 168318), 231));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 168663), 143));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 169007), 25));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 169179), 28));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 169351), 124));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 169524), 132));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 169696), 24));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 169869), 29));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 170041), 124));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 170213), 133));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 170386), 459));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 170558), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 170731), 440));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 170903), 262));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 171075), 482));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 171248), 237));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 171420), 448));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 171592), 270));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 171765), 359));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 171851), 359));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 171938), 359));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 172110), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 172196), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 172283), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 172455), 340));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 172800), 263));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 173144), 342));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 174007), 155));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 174179), 170));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 174265), 240));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 174351), 310));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 174438), 381));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 174524), 452));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 174610), 473));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 174696), 494));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 174869), 413));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 174955), 392));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 175127), 287));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 175300), 250));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 175386), 249));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 175558), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 175644), 175));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 175731), 201));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 175817), 209));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 175989), 86));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 176075), 67));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 176248), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 176334), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 176420), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 176593), 71));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 176679), 109));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 176765), 150));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 176851), 191));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 176938), 261));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 177024), 299));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 177110), 340));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 177196), 381));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 177282), 444));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 177369), 450));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 177455), 456));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 177627), 375));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 177713), 347));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 177886), 476));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 178058), 354));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 178144), 312));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 178317), 222));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 178403), 187));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 178489), 145));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 178575), 107));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 178748), 160));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 178834), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 179007), 57));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 179093), 57));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 179179), 57));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 179351), 131));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 179438), 172));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 179524), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 179696), 375));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 179783), 416));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 179869), 457));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 180041), 441));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 180127), 441));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 180213), 441));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 180386), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 180472), 268));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 180644), 160));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 180817), 24));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 180903), 5));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 181075), 72));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 181162), 105));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 181248), 121));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 181334), 121));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 181507), 128));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 181593), 160));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 181765), 323));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 181851), 312));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 181938), 300));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 182110), 127));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 182196), 127));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 182282), 127));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 182455), 233));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 182541), 233));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 182627), 233));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 182800), 341));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 182886), 375));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 182972), 398));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 183144), 320));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 183231), 299));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 183403), 440));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 183575), 335));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 183662), 319));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 183834), 105));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 183920), 80));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 184007), 72));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 184093), 86));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 184265), 242));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 184351), 212));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 184524), 1));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 184696), 206));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 184782), 206));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 184869), 206));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 185213), 199));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 185472), 318));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 185558), 321));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 185644), 360));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 185731), 400));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 185903), 499));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 185990), 459));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 186162), 282));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 186334), 378));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 186421), 384));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 186593), 192));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 186679), 152));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 186765), 110));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 186851), 71));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 187024), 118));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 187110), 157));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 187282), 192));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 187369), 223));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 187455), 252));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 187627), 153));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 187713), 153));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 187800), 153));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 187972), 282));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 188058), 282));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 188145), 282));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 188317), 482));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 188403), 502));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 188489), 505));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 188662), 389));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 188748), 386));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 188920), 380));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 189093), 295));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 189179), 258));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 189351), 67));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 189438), 40));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 189524), 39));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 189610), 58));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 189782), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 189869), 177));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 190041), 30));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 190127), 30));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 190213), 30));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 190386), 225));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 190558), 154));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 190731), 275));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 191075), 234));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 191248), 287));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 191334), 287));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 191420), 287));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 191593), 462));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 191679), 462));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 191765), 462));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 191938), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 192024), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 192110), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 192282), 489));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 192369), 489));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 192455), 489));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 192627), 333));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 192799), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 192971), 275));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 193143), 67));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 193317), 219));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 193489), 219));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 193834), 219));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 194007), 302));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 194179), 123));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 194352), 40));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 194524), 297));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 194697), 380));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 194869), 201));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 195042), 118));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 195213), 297));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 195300), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 195386), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 195472), 326));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 195558), 336));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 195644), 346));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 195731), 356));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 195817), 366));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 195903), 377));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 196075), 442));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 196248), 272));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 196334), 223));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 196420), 229));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 196507), 287));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 196593), 355));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 196765), 354));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 196938), 374));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 197110), 221));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 197196), 218));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 197282), 215));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 197455), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 197627), 210));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 197800), 489));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 197972), 157));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 198144), 206));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 198317), 50));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 198489), 248));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 198575), 248));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 198662), 248));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 198834), 96));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 199007), 46));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 199179), 94));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 199351), 251));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 199696), 396));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 199869), 365));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 200041), 504));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 200213), 278));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 200386), 489));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 200558), 288));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 200731), 245));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 201075), 91));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 201248), 135));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 201420), 176));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 201507), 173));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 201593), 156));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 201679), 126));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 201765), 89));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 202110), 128));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 202282), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 202455), 112));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 202627), 301));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 202972), 271));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 203058), 271));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 203144), 271));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 203317), 378));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 203489), 383));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 203662), 241));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 203834), 460));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 204007), 241));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 204093), 218));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 204179), 197));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 204351), 268));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 204524), 62));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 204696), 273));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 204869), 59));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 205041), 67));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 205213), 255));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 205558), 191));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 205731), 431));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 205903), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 206075), 241));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 206248), 377));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 206420), 248));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 206593), 252));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 206938), 117));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 207110), 225));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 207196), 234));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 207455), 251));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 207627), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 207800), 56));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 207886), 45));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 207972), 46));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 208058), 61));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 208144), 89));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 208231), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 208317), 159));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 208489), 67));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 208662), 117));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 208834), 252));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 208920), 252));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 209007), 252));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 209179), 89));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 209351), 347));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 209524), 169));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 209696), 0));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 209869), 266));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 210041), 89));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 210213), 164));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 210386), 357));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 210731), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 210903), 469));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 211075), 277));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 211248), 370));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 211420), 261));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 211593), 245));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 211765), 159));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 212110), 159));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 212196), 124));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 212282), 87));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 212369), 51));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 212455), 17));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 212541), 19));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 212627), 21));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 212713), 23));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 212800), 23));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 213144), 23));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 213317), 242));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 213489), 23));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 213662), 131));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 213835), 373));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 214007), 154));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 214179), 227));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 214352), 469));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 214524), 504));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 214697), 262));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 214869), 481));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 215042), 373));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 215213), 131));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 215387), 350));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 215558), 233));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 215644), 199));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 215731), 162));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 215817), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 215903), 91));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 216075), 134));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 216248), 87));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 216420), 29));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 216593), 84));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 216765), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 216938), 82));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 217110), 12));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 217282), 130));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 217369), 165));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 217455), 201));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 217541), 237));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 217627), 268));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 217713), 292));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 217800), 353));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 217886), 420));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 217972), 465));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 218058), 459));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 218144), 388));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 218231), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 218662), 307));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 219007), 193));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 219179), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 219351), 349));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 219524), 172));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 219696), 349));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 219955), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 220041), 290));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 220213), 253));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 220386), 390));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 220644), 239));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 220731), 254));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 220817), 262));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 220903), 258));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 221075), 409));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 221248), 127));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 221420), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 221593), 226));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 221765), 121));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 222024), 145));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 222110), 147));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 222282), 22));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 222455), 214));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 222627), 26));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 222800), 127));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 223058), 293));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 223144), 300));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 223317), 389));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 223403), 391));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 223489), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 223575), 439));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 223662), 475));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 223834), 241));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 224007), 464));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 224179), 471));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 224524), 471));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 224783), 327));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 224869), 337));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 224955), 333));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 225042), 314));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 225213), 420));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 225386), 246));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 225558), 246));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 225903), 313));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 226075), 190));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 226248), 392));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 226420), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 226593), 404));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 226765), 244));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 226938), 184));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 227110), 184));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 227282), 140));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 227541), 278));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 227627), 288));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 227713), 298));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 227800), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 227972), 180));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 228144), 138));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 228317), 186));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 228489), 71));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 228662), 231));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 228834), 231));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 229007), 81));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 229179), 283));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 229351), 69));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 229524), 101));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 229610), 101));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 229696), 101));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 230041), 111));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 230558), 101));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 230731), 198));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 230989), 183));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 231075), 183));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 231248), 297));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 231420), 297));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 231679), 382));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 231938), 288));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 232110), 288));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 232455), 365));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 232627), 365));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 232800), 462));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 233058), 464));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 233144), 464));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 233317), 285));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 233489), 339));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 233834), 446));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 234007), 358));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 234179), 263));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 234438), 285));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 234696), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 234869), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 235213), 41));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 235558), 49));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 236075), 55));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 236248), 55));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 236593), 144));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 236765), 144));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 236938), 231));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 237110), 233));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 237627), 182));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 237972), 347));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 238144), 347));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 238317), 342));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 238834), 347));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 239007), 427));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 239524), 451));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 239696), 435));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 239869), 419));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 240041), 404));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 240213), 338));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 240558), 266));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 240731), 221));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 243144), 220));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 243489), 110));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 244869), 221));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 245558), 226));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 246248), 242));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 248662), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 249007), 139));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 250041), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 250386), 247));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 251075), 159));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 251765), 182));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 254524), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 255903), 80));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 256593), 243));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 257282), 25));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 260041), 248));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2659 + 261420), 243));
        }

        internal void pause(GameTime gameTime)
        {
            IsPlaying = false;
            MediaPlayer.Pause();
            pauseTime = gameTime.TotalGameTime;
        }

        //void level2()
        //{
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2000), playAreaSector * 1));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2200), playAreaSector * 3));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2300), playAreaSector * 2));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2400), playAreaSector * 1));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(2600), playAreaSector * 2));

        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(3000), playAreaSector * 3));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(3200), playAreaSector * 1));

        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(4000), playAreaSector * 5));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(4200), playAreaSector * 7));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(4300), playAreaSector * 6));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(4400), playAreaSector * 5));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(4600), playAreaSector * 6));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5000), playAreaSector * 7));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5200), playAreaSector * 5));

        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5300), playAreaSector * 3));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5400), playAreaSector * 6));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5450), playAreaSector * 5));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5500), playAreaSector * 3));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5600), playAreaSector * 10));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5700), playAreaSector * 3));
        //    enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(5800), playAreaSector * 10));
        //}

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
