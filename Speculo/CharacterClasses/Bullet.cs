using Microsoft.Xna.Framework;
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
        Texture2D bulletTexture;
        const float bulletVelocity = 2f;
        TimeSpan bulletShot;
        public bool IsProjectileDead { get; set; }
        //static private Texture2D characterBorder;

        public Bullet(Vector2 bulletStartPos, Texture2D projectileTexture, GraphicsDevice g)
        {
            this.direction = new Vector2(0, 1);
            this.bulletStartPos = bulletStartPos;
            this.bulletTexture = projectileTexture;
            this.bulletPosition = bulletStartPos;
            IsProjectileDead = false;
            bulletShot = sharedVariables.gameTime.TotalGameTime;
        }

        public void Update(GameTime gameTime)
        {
            bulletPosition += direction * bulletVelocity; 

            if (sharedVariables.gameTime.TotalGameTime > bulletShot + TimeSpan.FromSeconds(2))
            {
                IsProjectileDead = true;
            }
            bulletCollision();


        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(bulletTexture, bulletPosition, Color.White);
        }

        public void bulletCollision()
        {
            Rectangle newRectangle = new Rectangle((int)sharedVariables.GamePlay.CharacterClass.Position.X, (int)sharedVariables.GamePlay.CharacterClass.Position.Y, sharedVariables.GamePlay.CharacterClass.Rectangle.Width, sharedVariables.GamePlay.CharacterClass.Rectangle.Height);
            Rectangle rect = new Rectangle((int)bulletPosition.X, (int)bulletPosition.Y, bulletTexture.Width, bulletTexture.Height);

            if (rect.TouchTopOf(newRectangle) || rect.TouchBottomOf(newRectangle) || rect.TouchRightOf(newRectangle) || rect.TouchLeftOf(newRectangle))
            {
                IsProjectileDead = true;
            }
        }
    }
}
