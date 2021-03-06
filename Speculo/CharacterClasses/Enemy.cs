﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Speculo.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speculo.CharacterClasses
{
    public class Enemy
    {
        public List<Bullet> Projectiles;
        List<Bullet> projectilesToRemove;


        private Texture2D texture;
        private Vector2 position;

        private TimeSpan approachTime;
        private float yAmount;
        private float xPos;
        private float yPos;

        private Rectangle enemyRectangle;

        SharedVariables sharedVariables = SharedVariables.Instance;
        private bool hasShot;
        private bool enemySent;
        private bool isDead;
        private SoundEffect dieSound;
        private SoundEffect comboBreakSound;

        private Vector2 direction;
        private float velocity;

        public Rectangle EnemyRectangle
        {
            get { return enemyRectangle; }
            set { enemyRectangle = value; }
        }

        public TimeSpan ApproachTime
        {
            get { return approachTime; }
            set { approachTime = value; }
        }

        public bool EnemySent
        {
            get { return enemySent; }
            set { enemySent = value; }
        }

        public bool IsDead
        {
            get { return isDead; }
            set { isDead = value; }
        }

        public Vector2 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        public static Texture2D ProjectileTexture { get; set; }


        public Enemy(TimeSpan approachTime, float xPos)
        {
            if (texture == null)
            {
                texture = sharedVariables.Content.Load<Texture2D>("Textures/Enemy");
                ProjectileTexture = sharedVariables.Content.Load<Texture2D>("Textures/Gameplay/Projectiles/projectile3");
                dieSound = sharedVariables.Content.Load<SoundEffect>("Sound/Gameplay/enemyDie");
                comboBreakSound = sharedVariables.Content.Load<SoundEffect>("Sound/Gameplay/combobreak");
            }
            this.approachTime = approachTime;
            this.xPos = xPos;
            initialize(xPos);

            
        }

        public void initialize(float xPos)
        {
            isDead = false;
            enemySent = false;
            hasShot = false;

            position = new Vector2(0, 0);
            enemyRectangle = new Rectangle((int)position.X, (int)position.Y,
                (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 20,
                (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 25);

             direction = new Vector2(0, 1.2f);

            //450 = 900/2; 2 = default velocity
            velocity = sharedVariables.GraphicsManager.PreferredBackBufferHeight / 450;

            //enemyRectangle = new Rectangle(
            //    (int)position.X,
            //    (int)position.Y,
            //    ((int)sharedVariables.Game[sharedVariables.ScreenSizeIndex].X - ((int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 100) * 32) / 12,
            //    (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 20
            //    );

            position = new Vector2(xPos, yPos);

            yAmount = sharedVariables.GraphicsManager.PreferredBackBufferHeight / 2;

            Projectiles = new List<Bullet>();
            projectilesToRemove = new List<Bullet>();
        }

        public void Update(GameTime gameTime)
        {
            if (sharedVariables.GamePlay.GameRuntime  + sharedVariables.GamePlay.GameStartTime > sharedVariables.GamePlay.GameStartTime + approachTime)
            {
                enemySent = true;
                sendEnemy(gameTime);
            }

            updateProjectileTravel(gameTime);
            removeProjectiles();
        }

        void updateProjectileTravel(GameTime gameTime)
        {
            foreach (Bullet proj in Projectiles)
            {
                if (!proj.IsProjectileDead)
                {
                    proj.Update(gameTime);
                }
                else
                {
                    projectilesToRemove.Add(proj);
                    isDead = true;
                    sharedVariables.GamePlay.breakCombo();
                    comboBreakSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                }
            }
        }

        void removeProjectiles()
        {
            foreach (Bullet proj in projectilesToRemove)
            {
                Projectiles.Remove(proj);
            }
            projectilesToRemove.Clear();
        }

        public void sendEnemy(GameTime gameTime)
        {
            if(!hasShot)
            {
                position.X = sharedVariables.GamePlay.PlayArea.X + xPos;
                position += direction * velocity;

                if (position.Y >= yAmount)
                {
                    hasShot = true;
                    
                    shoot(gameTime);
                }
            }
        }

        public void shoot(GameTime gameTime)
        {
            Bullet projectile = new Bullet(new Vector2(position.X + enemyRectangle.Width / 2, position.Y), sharedVariables.GamePlay.GameRuntime,
            ProjectileTexture, sharedVariables.Graphics, this);
            Projectiles.Add(projectile);
        }

        public void getHit()
        {
            this.isDead = true;
            
            sharedVariables.GamePlay.killedEnemy();
            //dieSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(enemySent)
            {
                spriteBatch.Draw(texture, new Rectangle((int)position.X, (int)position.Y, enemyRectangle.Width, enemyRectangle.Height), Color.White);
            }

            foreach (Bullet proj in Projectiles)
            {
                proj.Draw(spriteBatch);
            }

        }
    }
}
