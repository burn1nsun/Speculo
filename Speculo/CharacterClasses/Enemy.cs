﻿using Microsoft.Xna.Framework;
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
        public List<Bullet> Projectiles = new List<Bullet>();
        List<Bullet> projectilesToRemove = new List<Bullet>();


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

        public Rectangle EnemyRectangle
        {
            get { return enemyRectangle; }
            set { enemyRectangle = value; }
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
                ProjectileTexture = sharedVariables.Content.Load<Texture2D>("Textures/Projectiles/Projectile1");
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
            enemyRectangle = new Rectangle((int)position.X, (int)position.Y, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].X / 20, (int)sharedVariables.ScreenSizes[sharedVariables.ScreenSizeIndex].Y / 25);
            position = new Vector2(xPos, yPos);

            yAmount = sharedVariables.GraphicsManager.PreferredBackBufferHeight / 2;
        }

        public void Update(GameTime gameTime)
        {
            if (gameTime.TotalGameTime > sharedVariables.GamePlay.GameStartTime + approachTime)
            {
                enemySent = true;
                sendEnemy();
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

        public void sendEnemy()
        {
            if(!hasShot)
            {
                position = new Vector2(sharedVariables.GamePlay.PlayArea.X + xPos, yPos);
                yPos++;
                if (yPos >= yAmount)
                {
                    hasShot = true;
                    shoot();
                }
            }
        }

        public void shoot()
        {
            Bullet projectile = new Bullet(position,
            ProjectileTexture, sharedVariables.Graphics, this);
            Projectiles.Add(projectile);
        }

        public void getHit()
        {
            this.isDead = true;
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
