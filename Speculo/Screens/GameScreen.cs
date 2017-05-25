using Microsoft.Xna.Framework;
using Speculo.CharacterClasses;
using Speculo.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Speculo.Screens
{
    public class GameScreen : BaseScreen
    {
        SharedVariables sharedVariables = SharedVariables.Instance;

        private Character character;

        public GameScreen(Game1 game)
        {
            Content = game.Content;

            character = sharedVariables.CharacterClass;

        }
        public override void Update(GameTime gameTime)
        {
            sharedVariables.CharacterClass.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            sharedVariables.CharacterClass.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
