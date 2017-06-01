using Microsoft.Xna.Framework;
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
    public class Bullet
    {
        SharedVariables sharedVariables = SharedVariables.Instance;

        Vector2 bulletStartPos;
        Vector2 bulletPosition;
        Vector2 direction;
        Rectangle bulletRectangle;
        Texture2D bulletTexture;
        float bulletVelocity;
        TimeSpan bulletShot;
        Enemy enemy;

        private SoundEffect bounceBackSound;

        private bool hasBounced;

        public Rectangle BulletRectangle { get { return bulletRectangle; } set { bulletRectangle = value; } }
        public bool IsProjectileDead { get; set; }
        //static private Texture2D characterBorder;

        public Bullet(Vector2 bulletStartPos, TimeSpan bulletShot, Texture2D projectileTexture, GraphicsDevice g, Enemy enemy)
        {
            this.enemy = enemy;
            this.direction = new Vector2(0, 1.5f);
            this.bulletStartPos = bulletStartPos;
            this.bulletTexture = projectileTexture;
            this.bulletPosition = new Vector2(bulletStartPos.X - projectileTexture.Width / 2, bulletStartPos.Y);
            this.bulletShot = bulletShot;

            //450 = 900/2; 2 = default velocity
            this.bulletVelocity = sharedVariables.GraphicsManager.PreferredBackBufferHeight / 450;
            IsProjectileDead = false;
            bounceBackSound = sharedVariables.Content.Load<SoundEffect>("Sound/Gameplay/catchBullet");
            initialize();
        }

        public void initialize()
        {
            BulletRectangle = new Rectangle(0, 0, (int)((sharedVariables.GraphicsManager.PreferredBackBufferWidth / 64)), (int)((sharedVariables.GraphicsManager.PreferredBackBufferHeight / 36)));
            IsProjectileDead = false;
        }

        public void Update(GameTime gameTime)
        {
            bulletPosition += direction * bulletVelocity; 


            //if (sharedVariables.GamePlay.GameRuntime > bulletShot + TimeSpan.FromSeconds(2))
            //{
            //    IsProjectileDead = true;
            //}
            bulletCollision();
            hitBottom();
        }

        public void hitBottom()
        {
            if(bulletPosition.Y >= sharedVariables.GamePlay.PlayArea.Height)
            {
                IsProjectileDead = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(bulletTexture, new Rectangle((int)bulletPosition.X, (int)bulletPosition.Y, bulletRectangle.Width, bulletRectangle.Height), Color.White);
        }

        public void bounceBack()
        {
            this.direction *= -1;
            hasBounced = true;
        }

        public void bulletCollision()
        {
            Rectangle rect = new Rectangle((int)bulletPosition.X, (int)bulletPosition.Y, bulletRectangle.Width, bulletRectangle.Height);
            if (!hasBounced)
            {
                Rectangle newRectangle = new Rectangle((int)sharedVariables.GamePlay.CharacterClass.Position.X, (int)sharedVariables.GamePlay.CharacterClass.Position.Y, sharedVariables.GamePlay.CharacterClass.Rectangle.Width, sharedVariables.GamePlay.CharacterClass.Rectangle.Height);

                if (rect.TouchTopOf(newRectangle))
                {
                    bounceBack();
                    bounceBackSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                }
            }

            else if(hasBounced)
            {
                Rectangle newRectangle = new Rectangle((int)enemy.Position.X, (int)enemy.Position.Y, enemy.EnemyRectangle.Width, enemy.EnemyRectangle.Height);

                if (rect.TouchBottomOf(newRectangle))
                {
                    IsProjectileDead = true;
                    enemy.getHit();
                }
            }
        }
    }
}
