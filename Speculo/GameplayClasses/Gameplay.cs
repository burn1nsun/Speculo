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
        private int biggestCombo;

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

        public int BiggestCombo
        {
            get { return biggestCombo; }
            set { this.biggestCombo = value; }
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
            biggestCombo = 0;
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
            biggestCombo = 0;
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

            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(9007 - 1992), 147));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(11765 - 1992), 268));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(13144 - 1992), 195));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(13834 - 1992), 140));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(14524 - 1992), 339));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(17282 - 1992), 410));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(18662 - 1992), 264));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(20041 - 1992), 104));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(20213 - 1992), 167));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(20558 - 1992), 85));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(20903 - 1992), 152));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(21248 - 1992), 254));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(21334 - 1992), 255));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(21420 - 1992), 256));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(21593 - 1992), 327));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(21679 - 1992), 326));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(21765 - 1992), 325));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(21938 - 1992), 236));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(22024 - 1992), 237));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(22110 - 1992), 238));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(22283 - 1992), 309));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(22369 - 1992), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(22455 - 1992), 307));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(22800 - 1992), 413));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(22972 - 1992), 350));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(23317 - 1992), 432));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(23662 - 1992), 365));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(24007 - 1992), 251));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(24093 - 1992), 249));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(24179 - 1992), 247));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(24351 - 1992), 159));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(24524 - 1992), 136));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(24869 - 1992), 139));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(25041 - 1992), 34));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(25213 - 1992), 18));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(25558 - 1992), 136));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(25903 - 1992), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(26420 - 1992), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(26593 - 1992), 179));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(26938 - 1992), 216));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(27110 - 1992), 216));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(27455 - 1992), 299));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(27713 - 1992), 304));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(27800 - 1992), 304));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(27972 - 1992), 391));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(28231 - 1992), 499));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(28317 - 1992), 504));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(28489 - 1992), 496));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(28662 - 1992), 393));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(28921 - 1992), 413));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(29007 - 1992), 410));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(29093 - 1992), 407));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(29179 - 1992), 404));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(29351 - 1992), 271));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(29610 - 1992), 374));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(29696 - 1992), 379));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(29869 - 1992), 393));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(30041 - 1992), 393));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(30213 - 1992), 251));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(30472 - 1992), 279));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(30731 - 1992), 135));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(30903 - 1992), 53));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(31075 - 1992), 69));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(31248 - 1992), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(31420 - 1992), 226));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(31679 - 1992), 207));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(31765 - 1992), 234));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(31851 - 1992), 264));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(31938 - 1992), 295));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(32110 - 1992), 512));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(32455 - 1992), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(32627 - 1992), 226));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(32800 - 1992), 226));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(32972 - 1992), 295));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(33058 - 1992), 297));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(33144 - 1992), 307));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(33231 - 1992), 328));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(33317 - 1992), 354));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(33489 - 1992), 133));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(33834 - 1992), 56));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(34007 - 1992), 225));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(34179 - 1992), 44));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(34438 - 1992), 171));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(34524 - 1992), 158));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(34610 - 1992), 157));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(34696 - 1992), 163));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(34869 - 1992), 371));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(35213 - 1992), 357));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(35386 - 1992), 291));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(35558 - 1992), 432));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(35731 - 1992), 443));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(35817 - 1992), 443));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(36075 - 1992), 464));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(36248 - 1992), 321));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(36420 - 1992), 512));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(36593 - 1992), 371));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(36938 - 1992), 81));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(37196 - 1992), 78));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(37282 - 1992), 106));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(37369 - 1992), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(37455 - 1992), 168));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(37627 - 1992), 65));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(37972 - 1992), 174));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(38144 - 1992), 186));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(38317 - 1992), 186));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(38489 - 1992), 259));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(38575 - 1992), 258));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(38662 - 1992), 257));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(38748 - 1992), 255));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(38834 - 1992), 254));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(39007 - 1992), 292));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(39179 - 1992), 168));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(39351 - 1992), 391));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(39696 - 1992), 444));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(39869 - 1992), 313));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(39955 - 1992), 341));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(40041 - 1992), 372));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(40127 - 1992), 402));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(40213 - 1992), 431));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(40386 - 1992), 501));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(40731 - 1992), 310));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(40903 - 1992), 348));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(41075 - 1992), 348));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(41248 - 1992), 228));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(41420 - 1992), 266));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(41593 - 1992), 236));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(41765 - 1992), 146));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(41938 - 1992), 56));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(42024 - 1992), 104));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(42110 - 1992), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(42455 - 1992), 66));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(42627 - 1992), 162));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(42713 - 1992), 190));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(42800 - 1992), 221));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(42886 - 1992), 251));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(42972 - 1992), 280));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(43144 - 1992), 468));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(43489 - 1992), 228));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(43662 - 1992), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(43834 - 1992), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(44007 - 1992), 32));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(44093 - 1992), 61));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(44179 - 1992), 91));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(44266 - 1992), 122));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(44352 - 1992), 150));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(44524 - 1992), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(44869 - 1992), 309));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(45041 - 1992), 420));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(45213 - 1992), 229));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(45471 - 1992), 358));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(45557 - 1992), 357));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(45644 - 1992), 361));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(45730 - 1992), 373));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(45903 - 1992), 408));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(46248 - 1992), 419));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(46420 - 1992), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(46593 - 1992), 494));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(46765 - 1992), 300));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(46851 - 1992), 300));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(47110 - 1992), 319));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(47282 - 1992), 216));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(47455 - 1992), 419));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(47627 - 1992), 165));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(47972 - 1992), 165));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(48144 - 1992), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(48231 - 1992), 133));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(48317 - 1992), 128));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(48403 - 1992), 117));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(48489 - 1992), 95));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(48662 - 1992), 209));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(49007 - 1992), 286));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(49179 - 1992), 341));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(49351 - 1992), 165));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(49524 - 1992), 398));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(49610 - 1992), 399));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(49696 - 1992), 400));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(49782 - 1992), 402));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(49869 - 1992), 403));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(50041 - 1992), 414));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(50386 - 1992), 195));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(50472 - 1992), 191));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(50558 - 1992), 188));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(50644 - 1992), 185));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(50731 - 1992), 183));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(50817 - 1992), 182));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(50903 - 1992), 182));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(50989 - 1992), 183));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(51075 - 1992), 184));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(51162 - 1992), 156));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(51248 - 1992), 111));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(51334 - 1992), 75));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(51420 - 1992), 67));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(51507 - 1992), 86));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(51593 - 1992), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(51679 - 1992), 167));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(51765 - 1992), 249));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(52455 - 1992), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(53144 - 1992), 141));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(53834 - 1992), 90));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(54524 - 1992), 43));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(55041 - 1992), 93));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(55558 - 1992), 149));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(56248 - 1992), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(56420 - 1992), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(56593 - 1992), 277));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(56765 - 1992), 361));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(56938 - 1992), 367));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(57282 - 1992), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(57799 - 1992), 361));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(58316 - 1992), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(59007 - 1992), 240));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(59179 - 1992), 464));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(59351 - 1992), 201));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(59524 - 1992), 401));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(59696 - 1992), 272));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(60041 - 1992), 228));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(60558 - 1992), 278));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(61075 - 1992), 334));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(61765 - 1992), 394));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(62110 - 1992), 476));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(62282 - 1992), 267));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(62455 - 1992), 279));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(62800 - 1992), 195));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(63144 - 1992), 382));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(63317 - 1992), 139));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(63489 - 1992), 332));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(63662 - 1992), 190));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(63834 - 1992), 361));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(64179 - 1992), 437));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(64869 - 1992), 381));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(65213 - 1992), 216));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(65386 - 1992), 498));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(65558 - 1992), 274));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(66075 - 1992), 267));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(66593 - 1992), 285));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(67282 - 1992), 327));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(67627 - 1992), 402));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(67972 - 1992), 236));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(68144 - 1992), 477));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(68317 - 1992), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(68834 - 1992), 304));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(69352 - 1992), 285));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(70041 - 1992), 263));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(70386 - 1992), 277));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(70558 - 1992), 330));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(70903 - 1992), 495));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(71075 - 1992), 311));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(71592 - 1992), 303));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(72110 - 1992), 330));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(72627 - 1992), 413));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(72800 - 1992), 338));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(73144 - 1992), 212));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(73317 - 1992), 37));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(73489 - 1992), 37));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(73833 - 1992), 162));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(74007 - 1992), 235));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(74179 - 1992), 235));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(74523 - 1992), 110));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(74696 - 1992), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(74869 - 1992), 285));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(75213 - 1992), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(75386 - 1992), 430));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(75558 - 1992), 442));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(75731 - 1992), 384));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(75903 - 1992), 396));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(76075 - 1992), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(76248 - 1992), 354));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(76420 - 1992), 366));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(76592 - 1992), 381));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(76765 - 1992), 327));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(76937 - 1992), 339));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(77109 - 1992), 354));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(77282 - 1992), 293));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(77454 - 1992), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(77626 - 1992), 320));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(77800 - 1992), 320));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(77972 - 1992), 302));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(78145 - 1992), 168));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(78317 - 1992), 156));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(78490 - 1992), 214));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(78662 - 1992), 202));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(78834 - 1992), 187));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(79007 - 1992), 244));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(79179 - 1992), 232));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(79351 - 1992), 217));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(79524 - 1992), 271));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(79696 - 1992), 259));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(79868 - 1992), 244));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(80041 - 1992), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(80213 - 1992), 293));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(80385 - 1992), 278));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(80559 - 1992), 278));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(80731 - 1992), 164));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(80903 - 1992), 140));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(81075 - 1992), 91));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(81248 - 1992), 60));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(81420 - 1992), 269));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(81593 - 1992), 293));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(81764 - 1992), 342));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(81937 - 1992), 373));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(82110 - 1992), 455));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(82282 - 1992), 456));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(82455 - 1992), 198));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(82627 - 1992), 192));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(82800 - 1992), 470));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(82972 - 1992), 473));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(83144 - 1992), 172));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(83316 - 1992), 161));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(83489 - 1992), 431));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(83575 - 1992), 409));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(83662 - 1992), 383));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(83748 - 1992), 357));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(83834 - 1992), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(83920 - 1992), 294));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(84007 - 1992), 268));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(84093 - 1992), 242));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(84179 - 1992), 196));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(84524 - 1992), 161));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(84869 - 1992), 116));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(85731 - 1992), 224));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(85903 - 1992), 268));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(85989 - 1992), 267));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(86075 - 1992), 264));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(86162 - 1992), 263));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(86248 - 1992), 283));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(86420 - 1992), 352));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(86593 - 1992), 195));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(86765 - 1992), 101));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(86851 - 1992), 108));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(86938 - 1992), 116));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(87110 - 1992), 352));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(87282 - 1992), 85));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(87455 - 1992), 159));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(87627 - 1992), 189));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(87800 - 1992), 61));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(87972 - 1992), 77));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(88144 - 1992), 229));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(88231 - 1992), 266));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(88317 - 1992), 298));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(88403 - 1992), 324));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(88489 - 1992), 340));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(88662 - 1992), 376));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(88834 - 1992), 353));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(89007 - 1992), 373));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(89179 - 1992), 424));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(89351 - 1992), 266));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(89524 - 1992), 425));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(89696 - 1992), 222));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(89869 - 1992), 449));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(90041 - 1992), 203));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(90213 - 1992), 216));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(90386 - 1992), 317));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(90558 - 1992), 79));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(90731 - 1992), 325));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(90817 - 1992), 339));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(90903 - 1992), 328));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(90989 - 1992), 289));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(91075 - 1992), 241));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(91162 - 1992), 267));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(91248 - 1992), 289));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(91334 - 1992), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(91420 - 1992), 315));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(91765 - 1992), 304));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(91938 - 1992), 451));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(92110 - 1992), 238));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(92282 - 1992), 430));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(92455 - 1992), 433));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(92541 - 1992), 433));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(92627 - 1992), 433));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(92800 - 1992), 304));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(92972 - 1992), 126));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(93144 - 1992), 315));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(93317 - 1992), 126));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(93489 - 1992), 198));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(93662 - 1992), 145));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(93748 - 1992), 98));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(93834 - 1992), 61));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(93920 - 1992), 47));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(94007 - 1992), 54));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(94179 - 1992), 180));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(94351 - 1992), 126));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(94524 - 1992), 324));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(94696 - 1992), 61));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(94869 - 1992), 323));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(95127 - 1992), 255));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(95213 - 1992), 264));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(95386 - 1992), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(95558 - 1992), 218));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(95731 - 1992), 271));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(95903 - 1992), 415));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(96075 - 1992), 410));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(96248 - 1992), 114));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(96507 - 1992), 285));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(96593 - 1992), 302));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(96765 - 1992), 169));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(96938 - 1992), 187));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(97282 - 1992), 358));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(97455 - 1992), 415));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(97627 - 1992), 363));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(97800 - 1992), 245));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(97886 - 1992), 245));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(97972 - 1992), 245));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(98144 - 1992), 423));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(98317 - 1992), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(98489 - 1992), 241));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(98662 - 1992), 219));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(98834 - 1992), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(98920 - 1992), 420));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(99007 - 1992), 419));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(99179 - 1992), 186));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(99351 - 1992), 424));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(99524 - 1992), 329));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(99696 - 1992), 288));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(99869 - 1992), 322));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(100041 - 1992), 500));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(100213 - 1992), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(100386 - 1992), 492));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(100558 - 1992), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(100731 - 1992), 418));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(100903 - 1992), 225));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(101075 - 1992), 499));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(101248 - 1992), 144));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(101420 - 1992), 383));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(101765 - 1992), 75));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(101851 - 1992), 68));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(101938 - 1992), 77));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(102024 - 1992), 100));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(102110 - 1992), 134));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(102196 - 1992), 114));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(102282 - 1992), 100));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(102369 - 1992), 94));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(102455 - 1992), 95));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(102800 - 1992), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(103144 - 1992), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(103317 - 1992), 221));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(103489 - 1992), 250));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(103662 - 1992), 77));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(103834 - 1992), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(104007 - 1992), 163));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(104093 - 1992), 163));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(104179 - 1992), 163));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(104351 - 1992), 149));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(104524 - 1992), 325));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(104696 - 1992), 72));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(104869 - 1992), 327));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(105041 - 1992), 335));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(105213 - 1992), 358));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(105558 - 1992), 360));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(105731 - 1992), 454));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(105903 - 1992), 187));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(106248 - 1992), 358));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(106420 - 1992), 368));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(106593 - 1992), 267));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(106765 - 1992), 454));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(106938 - 1992), 150));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(107110 - 1992), 348));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(107282 - 1992), 311));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(107455 - 1992), 168));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(107627 - 1992), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(108317 - 1992), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(109007 - 1992), 176));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(109179 - 1992), 279));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(109265 - 1992), 279));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(109351 - 1992), 279));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(109524 - 1992), 194));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(109696 - 1992), 380));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(109782 - 1992), 380));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(109868 - 1992), 380));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(110041 - 1992), 209));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(110213 - 1992), 482));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(110386 - 1992), 249));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(110558 - 1992), 358));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(110903 - 1992), 227));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(111075 - 1992), 49));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(111420 - 1992), 180));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(111765 - 1992), 11));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(111938 - 1992), 19));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(112110 - 1992), 20));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(112455 - 1992), 28));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(112800 - 1992), 84));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(112972 - 1992), 183));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(113144 - 1992), 175));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(113317 - 1992), 196));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(113489 - 1992), 278));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(113662 - 1992), 40));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(113834 - 1992), 205));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(114524 - 1992), 315));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(114696 - 1992), 132));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(114782 - 1992), 132));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(114869 - 1992), 132));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(115041 - 1992), 264));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(115213 - 1992), 38));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(115299 - 1992), 38));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(115386 - 1992), 38));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(115558 - 1992), 307));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(115731 - 1992), 81));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(115903 - 1992), 351));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(116075 - 1992), 351));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(116420 - 1992), 339));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(116593 - 1992), 427));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(116938 - 1992), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(117282 - 1992), 206));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(117455 - 1992), 206));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(117627 - 1992), 201));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(117800 - 1992), 123));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(117972 - 1992), 350));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(118144 - 1992), 346));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(118317 - 1992), 346));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(118489 - 1992), 115));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(118661 - 1992), 119));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(118834 - 1992), 119));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(119007 - 1992), 399));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(119093 - 1992), 416));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(119179 - 1992), 379));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(119265 - 1992), 310));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(119351 - 1992), 281));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(120041 - 1992), 229));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(120213 - 1992), 466));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(120386 - 1992), 156));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(120558 - 1992), 416));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(120730 - 1992), 179));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(120903 - 1992), 489));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(121075 - 1992), 135));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(121248 - 1992), 372));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(121420 - 1992), 95));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(121593 - 1992), 37));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(121851 - 1992), 228));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(122110 - 1992), 45));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(122455 - 1992), 95));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(122800 - 1992), 110));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(122972 - 1992), 263));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(123144 - 1992), 91));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(123316 - 1992), 244));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(123489 - 1992), 69));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(123834 - 1992), 149));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(124007 - 1992), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(124093 - 1992), 338));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(124179 - 1992), 368));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(124351 - 1992), 176));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(124524 - 1992), 435));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(124696 - 1992), 115));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(124869 - 1992), 424));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(125558 - 1992), 508));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(125731 - 1992), 270));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(125903 - 1992), 477));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(126075 - 1992), 156));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(126248 - 1992), 394));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(126420 - 1992), 187));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(126593 - 1992), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(126765 - 1992), 204));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(126938 - 1992), 481));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(127282 - 1992), 455));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(127627 - 1992), 322));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(127800 - 1992), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(127972 - 1992), 116));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(128144 - 1992), 128));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(128317 - 1992), 341));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(128489 - 1992), 370));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(128662 - 1992), 95));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(128834 - 1992), 73));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(129007 - 1992), 188));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(129093 - 1992), 202));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(129179 - 1992), 217));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(129265 - 1992), 232));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(129351 - 1992), 247));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(129438 - 1992), 262));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(129524 - 1992), 277));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(129610 - 1992), 292));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(129696 - 1992), 307));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(129782 - 1992), 321));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(129869 - 1992), 335));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(130041 - 1992), 191));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(130213 - 1992), 353));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(130386 - 1992), 219));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(146938 - 1992), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(147627 - 1992), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(148317 - 1992), 272));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(148489 - 1992), 273));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(148662 - 1992), 274));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(148834 - 1992), 275));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(149007 - 1992), 350));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(149179 - 1992), 348));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(149351 - 1992), 346));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(149524 - 1992), 344));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(149696 - 1992), 281));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(149782 - 1992), 288));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(149869 - 1992), 295));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(149955 - 1992), 302));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(150041 - 1992), 309));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(150127 - 1992), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(150213 - 1992), 323));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(150300 - 1992), 330));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(150386 - 1992), 336));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(150472 - 1992), 307));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(150558 - 1992), 264));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(150644 - 1992), 226));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(150731 - 1992), 196));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(150817 - 1992), 153));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(150903 - 1992), 111));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(150989 - 1992), 82));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(151075 - 1992), 59));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(151765 - 1992), 123));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(152110 - 1992), 80));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(152455 - 1992), 58));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(153144 - 1992), 202));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(153834 - 1992), 154));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(154351 - 1992), 267));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(154869 - 1992), 380));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(155558 - 1992), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(155731 - 1992), 280));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(155903 - 1992), 192));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(156076 - 1992), 168));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(156248 - 1992), 105));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(156593 - 1992), 105));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(157110 - 1992), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(157627 - 1992), 170));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(158317 - 1992), 167));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(158662 - 1992), 173));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(158834 - 1992), 237));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(159007 - 1992), 336));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(159351 - 1992), 318));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(159869 - 1992), 393));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(160386 - 1992), 235));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(161075 - 1992), 280));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(161248 - 1992), 82));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(161334 - 1992), 53));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(161420 - 1992), 28));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(161507 - 1992), 23));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(161593 - 1992), 27));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(161679 - 1992), 37));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(161765 - 1992), 89));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(161852 - 1992), 94));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(161938 - 1992), 90));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(162024 - 1992), 80));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(162110 - 1992), 15));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(162282 - 1992), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(162455 - 1992), 207));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(162800 - 1992), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(162972 - 1992), 133));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(163144 - 1992), 292));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(163489 - 1992), 381));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(163834 - 1992), 293));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(164179 - 1992), 399));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(164524 - 1992), 311));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(164869 - 1992), 194));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(165214 - 1992), 106));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(165559 - 1992), 212));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(165904 - 1992), 124));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(166248 - 1992), 400));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(166593 - 1992), 312));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(166938 - 1992), 418));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(167283 - 1992), 330));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(167628 - 1992), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(167973 - 1992), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(168318 - 1992), 231));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(168663 - 1992), 143));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(169007 - 1992), 25));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(169179 - 1992), 28));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(169351 - 1992), 124));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(169524 - 1992), 132));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(169696 - 1992), 24));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(169869 - 1992), 29));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(170041 - 1992), 124));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(170213 - 1992), 133));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(170386 - 1992), 459));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(170558 - 1992), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(170731 - 1992), 440));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(170903 - 1992), 262));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(171075 - 1992), 482));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(171248 - 1992), 237));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(171420 - 1992), 448));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(171592 - 1992), 270));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(171765 - 1992), 359));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(171851 - 1992), 359));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(171938 - 1992), 359));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(172110 - 1992), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(172196 - 1992), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(172283 - 1992), 211));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(172455 - 1992), 340));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(172800 - 1992), 263));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(173144 - 1992), 342));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(174007 - 1992), 155));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(174179 - 1992), 170));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(174265 - 1992), 240));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(174351 - 1992), 310));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(174438 - 1992), 381));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(174524 - 1992), 452));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(174610 - 1992), 473));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(174696 - 1992), 494));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(174869 - 1992), 413));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(174955 - 1992), 392));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(175127 - 1992), 287));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(175300 - 1992), 250));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(175386 - 1992), 249));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(175558 - 1992), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(175644 - 1992), 175));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(175731 - 1992), 201));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(175817 - 1992), 209));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(175989 - 1992), 86));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(176075 - 1992), 67));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(176248 - 1992), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(176334 - 1992), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(176420 - 1992), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(176593 - 1992), 71));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(176679 - 1992), 109));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(176765 - 1992), 150));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(176851 - 1992), 191));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(176938 - 1992), 261));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(177024 - 1992), 299));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(177110 - 1992), 340));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(177196 - 1992), 381));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(177282 - 1992), 444));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(177369 - 1992), 450));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(177455 - 1992), 456));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(177627 - 1992), 375));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(177713 - 1992), 347));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(177886 - 1992), 476));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(178058 - 1992), 354));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(178144 - 1992), 312));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(178317 - 1992), 222));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(178403 - 1992), 187));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(178489 - 1992), 145));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(178575 - 1992), 107));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(178748 - 1992), 160));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(178834 - 1992), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(179007 - 1992), 57));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(179093 - 1992), 57));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(179179 - 1992), 57));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(179351 - 1992), 131));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(179438 - 1992), 172));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(179524 - 1992), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(179696 - 1992), 375));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(179783 - 1992), 416));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(179869 - 1992), 457));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(180041 - 1992), 441));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(180127 - 1992), 441));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(180213 - 1992), 441));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(180386 - 1992), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(180472 - 1992), 268));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(180644 - 1992), 160));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(180817 - 1992), 24));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(180903 - 1992), 5));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(181075 - 1992), 72));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(181162 - 1992), 105));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(181248 - 1992), 121));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(181334 - 1992), 121));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(181507 - 1992), 128));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(181593 - 1992), 160));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(181765 - 1992), 323));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(181851 - 1992), 312));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(181938 - 1992), 300));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(182110 - 1992), 127));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(182196 - 1992), 127));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(182282 - 1992), 127));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(182455 - 1992), 233));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(182541 - 1992), 233));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(182627 - 1992), 233));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(182800 - 1992), 341));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(182886 - 1992), 375));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(182972 - 1992), 398));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(183144 - 1992), 320));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(183231 - 1992), 299));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(183403 - 1992), 440));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(183575 - 1992), 335));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(183662 - 1992), 319));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(183834 - 1992), 105));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(183920 - 1992), 80));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(184007 - 1992), 72));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(184093 - 1992), 86));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(184265 - 1992), 242));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(184351 - 1992), 212));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(184524 - 1992), 1));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(184696 - 1992), 206));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(184782 - 1992), 206));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(184869 - 1992), 206));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(185213 - 1992), 199));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(185472 - 1992), 318));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(185558 - 1992), 321));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(185644 - 1992), 360));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(185731 - 1992), 400));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(185903 - 1992), 499));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(185990 - 1992), 459));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(186162 - 1992), 282));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(186334 - 1992), 378));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(186421 - 1992), 384));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(186593 - 1992), 192));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(186679 - 1992), 152));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(186765 - 1992), 110));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(186851 - 1992), 71));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(187024 - 1992), 118));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(187110 - 1992), 157));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(187282 - 1992), 192));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(187369 - 1992), 223));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(187455 - 1992), 252));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(187627 - 1992), 153));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(187713 - 1992), 153));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(187800 - 1992), 153));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(187972 - 1992), 282));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(188058 - 1992), 282));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(188145 - 1992), 282));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(188317 - 1992), 482));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(188403 - 1992), 502));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(188489 - 1992), 505));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(188662 - 1992), 389));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(188748 - 1992), 386));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(188920 - 1992), 380));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(189093 - 1992), 295));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(189179 - 1992), 258));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(189351 - 1992), 67));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(189438 - 1992), 40));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(189524 - 1992), 39));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(189610 - 1992), 58));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(189782 - 1992), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(189869 - 1992), 177));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(190041 - 1992), 30));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(190127 - 1992), 30));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(190213 - 1992), 30));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(190386 - 1992), 225));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(190558 - 1992), 154));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(190731 - 1992), 275));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(191075 - 1992), 234));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(191248 - 1992), 287));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(191334 - 1992), 287));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(191420 - 1992), 287));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(191593 - 1992), 462));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(191679 - 1992), 462));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(191765 - 1992), 462));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(191938 - 1992), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(192024 - 1992), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(192110 - 1992), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(192282 - 1992), 489));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(192369 - 1992), 489));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(192455 - 1992), 489));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(192627 - 1992), 333));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(192799 - 1992), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(192971 - 1992), 275));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(193143 - 1992), 67));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(193317 - 1992), 219));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(193489 - 1992), 219));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(193834 - 1992), 219));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(194007 - 1992), 302));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(194179 - 1992), 123));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(194352 - 1992), 40));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(194524 - 1992), 297));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(194697 - 1992), 380));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(194869 - 1992), 201));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(195042 - 1992), 118));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(195213 - 1992), 297));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(195300 - 1992), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(195386 - 1992), 316));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(195472 - 1992), 326));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(195558 - 1992), 336));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(195644 - 1992), 346));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(195731 - 1992), 356));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(195817 - 1992), 366));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(195903 - 1992), 377));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(196075 - 1992), 442));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(196248 - 1992), 272));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(196334 - 1992), 223));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(196420 - 1992), 229));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(196507 - 1992), 287));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(196593 - 1992), 355));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(196765 - 1992), 354));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(196938 - 1992), 374));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(197110 - 1992), 221));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(197196 - 1992), 218));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(197282 - 1992), 215));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(197455 - 1992), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(197627 - 1992), 210));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(197800 - 1992), 489));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(197972 - 1992), 157));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(198144 - 1992), 206));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(198317 - 1992), 50));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(198489 - 1992), 248));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(198575 - 1992), 248));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(198662 - 1992), 248));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(198834 - 1992), 96));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(199007 - 1992), 46));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(199179 - 1992), 94));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(199351 - 1992), 251));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(199696 - 1992), 396));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(199869 - 1992), 365));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(200041 - 1992), 504));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(200213 - 1992), 278));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(200386 - 1992), 489));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(200558 - 1992), 288));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(200731 - 1992), 245));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(201075 - 1992), 91));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(201248 - 1992), 135));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(201420 - 1992), 176));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(201507 - 1992), 173));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(201593 - 1992), 156));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(201679 - 1992), 126));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(201765 - 1992), 89));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(202110 - 1992), 128));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(202282 - 1992), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(202455 - 1992), 112));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(202627 - 1992), 301));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(202972 - 1992), 271));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(203058 - 1992), 271));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(203144 - 1992), 271));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(203317 - 1992), 378));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(203489 - 1992), 383));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(203662 - 1992), 241));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(203834 - 1992), 460));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(204007 - 1992), 241));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(204093 - 1992), 218));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(204179 - 1992), 197));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(204351 - 1992), 268));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(204524 - 1992), 62));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(204696 - 1992), 273));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(204869 - 1992), 59));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(205041 - 1992), 67));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(205213 - 1992), 255));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(205558 - 1992), 191));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(205731 - 1992), 431));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(205903 - 1992), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(206075 - 1992), 241));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(206248 - 1992), 377));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(206420 - 1992), 248));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(206593 - 1992), 252));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(206938 - 1992), 117));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(207110 - 1992), 225));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(207196 - 1992), 234));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(207455 - 1992), 251));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(207627 - 1992), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(207800 - 1992), 56));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(207886 - 1992), 45));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(207972 - 1992), 46));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(208058 - 1992), 61));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(208144 - 1992), 89));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(208231 - 1992), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(208317 - 1992), 159));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(208489 - 1992), 67));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(208662 - 1992), 117));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(208834 - 1992), 252));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(208920 - 1992), 252));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(209007 - 1992), 252));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(209179 - 1992), 89));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(209351 - 1992), 347));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(209524 - 1992), 169));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(209696 - 1992), 0));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(209869 - 1992), 266));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(210041 - 1992), 89));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(210213 - 1992), 164));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(210386 - 1992), 357));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(210731 - 1992), 213));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(210903 - 1992), 469));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(211075 - 1992), 277));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(211248 - 1992), 370));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(211420 - 1992), 261));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(211593 - 1992), 245));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(211765 - 1992), 159));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(212110 - 1992), 159));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(212196 - 1992), 124));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(212282 - 1992), 87));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(212369 - 1992), 51));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(212455 - 1992), 17));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(212541 - 1992), 19));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(212627 - 1992), 21));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(212713 - 1992), 23));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(212800 - 1992), 23));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(213144 - 1992), 20));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(213317 - 1992), 242));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(213489 - 1992), 23));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(213662 - 1992), 131));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(213835 - 1992), 373));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(214007 - 1992), 154));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(214179 - 1992), 227));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(214352 - 1992), 469));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(214524 - 1992), 504));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(214697 - 1992), 262));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(214869 - 1992), 481));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(215042 - 1992), 373));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(215213 - 1992), 131));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(215387 - 1992), 350));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(215558 - 1992), 233));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(215644 - 1992), 199));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(215731 - 1992), 162));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(215817 - 1992), 125));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(215903 - 1992), 91));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(216075 - 1992), 134));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(216248 - 1992), 87));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(216420 - 1992), 29));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(216593 - 1992), 84));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(216765 - 1992), 151));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(216938 - 1992), 82));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(217110 - 1992), 12));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(217282 - 1992), 130));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(217369 - 1992), 165));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(217455 - 1992), 201));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(217541 - 1992), 237));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(217627 - 1992), 268));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(217713 - 1992), 292));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(217800 - 1992), 353));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(217886 - 1992), 420));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(217972 - 1992), 465));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(218058 - 1992), 459));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(218144 - 1992), 388));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(218231 - 1992), 305));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(218662 - 1992), 307));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(219007 - 1992), 193));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(219179 - 1992), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(219351 - 1992), 349));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(219524 - 1992), 172));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(219696 - 1992), 349));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(219955 - 1992), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(220041 - 1992), 290));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(220213 - 1992), 253));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(220386 - 1992), 390));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(220644 - 1992), 239));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(220731 - 1992), 254));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(220817 - 1992), 262));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(220903 - 1992), 258));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(221075 - 1992), 409));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(221248 - 1992), 127));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(221420 - 1992), 296));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(221593 - 1992), 226));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(221765 - 1992), 121));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(222024 - 1992), 145));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(222110 - 1992), 147));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(222282 - 1992), 22));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(222455 - 1992), 214));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(222627 - 1992), 26));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(222800 - 1992), 127));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(223058 - 1992), 293));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(223144 - 1992), 300));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(223317 - 1992), 389));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(223403 - 1992), 391));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(223489 - 1992), 411));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(223575 - 1992), 439));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(223662 - 1992), 475));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(223834 - 1992), 241));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(224007 - 1992), 464));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(224179 - 1992), 471));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(224524 - 1992), 471));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(224783 - 1992), 327));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(224869 - 1992), 337));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(224955 - 1992), 333));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(225042 - 1992), 314));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(225213 - 1992), 420));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(225386 - 1992), 246));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(225558 - 1992), 246));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(225903 - 1992), 313));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(226075 - 1992), 190));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(226248 - 1992), 392));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(226420 - 1992), 181));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(226593 - 1992), 404));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(226765 - 1992), 244));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(226938 - 1992), 184));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(227110 - 1992), 184));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(227282 - 1992), 140));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(227541 - 1992), 278));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(227627 - 1992), 288));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(227713 - 1992), 298));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(227800 - 1992), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(227972 - 1992), 180));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(228144 - 1992), 138));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(228317 - 1992), 186));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(228489 - 1992), 71));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(228662 - 1992), 231));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(228834 - 1992), 231));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(229007 - 1992), 81));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(229179 - 1992), 283));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(229351 - 1992), 69));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(229524 - 1992), 101));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(229610 - 1992), 101));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(229696 - 1992), 101));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(230041 - 1992), 111));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(230558 - 1992), 101));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(230731 - 1992), 198));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(230989 - 1992), 183));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(231075 - 1992), 183));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(231248 - 1992), 297));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(231420 - 1992), 297));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(231679 - 1992), 382));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(231938 - 1992), 288));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(232110 - 1992), 288));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(232455 - 1992), 365));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(232627 - 1992), 365));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(232800 - 1992), 462));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(233058 - 1992), 464));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(233144 - 1992), 464));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(233317 - 1992), 285));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(233489 - 1992), 339));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(233834 - 1992), 446));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(234007 - 1992), 358));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(234179 - 1992), 263));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(234438 - 1992), 285));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(234696 - 1992), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(234869 - 1992), 137));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(235213 - 1992), 41));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(235558 - 1992), 49));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(236075 - 1992), 55));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(236248 - 1992), 55));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(236593 - 1992), 144));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(236765 - 1992), 144));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(236938 - 1992), 231));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(237110 - 1992), 233));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(237627 - 1992), 182));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(237972 - 1992), 347));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(238144 - 1992), 347));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(238317 - 1992), 342));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(238834 - 1992), 347));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(239007 - 1992), 427));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(239524 - 1992), 451));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(239696 - 1992), 435));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(239869 - 1992), 419));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(240041 - 1992), 404));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(240213 - 1992), 338));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(240558 - 1992), 266));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(240731 - 1992), 221));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(243144 - 1992), 220));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(243489 - 1992), 110));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(244869 - 1992), 221));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(245558 - 1992), 226));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(246248 - 1992), 242));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(248662 - 1992), 306));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(249007 - 1992), 139));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(250041 - 1992), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(250386 - 1992), 247));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(251075 - 1992), 159));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(251765 - 1992), 182));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(254524 - 1992), 308));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(255903 - 1992), 80));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(256593 - 1992), 243));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(257282 - 1992), 25));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(260041 - 1992), 248));
            enemyList.Add(new Enemy(TimeSpan.FromMilliseconds(261420 - 1992), 243));
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
            if(combo > biggestCombo)
            {
                biggestCombo = combo;
            }
            
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
