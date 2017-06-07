using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace Speculo.Screens
{
    public class LevelMenuScreen : BaseScreen
    {
        Game1 game;

        UserControls.Button backButton;
        Utility.SharedVariables sharedVariables = Utility.SharedVariables.Instance;
        List<UserControls.Control> Controls = new List<UserControls.Control>();
        MouseState presentMouse;

        private Texture2D hoverTexture;
        private Texture2D texture;

        private SoundEffect clickSound;
        private SoundEffect hoverSound;
        private SoundEffect backSound;

        
        public LevelMenuScreen(Game1 game)
        {
            this.game = game;
            loadContent();
            initializeButtons();
        }
        private void loadContent()
        {
            texture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Buttons/button1");
            hoverTexture = sharedVariables.Content.Load<Texture2D>("Textures/MenuObjects/Buttons/buttonOverlay");

            clickSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/menuclick");
            backSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/menuback");
            hoverSound = sharedVariables.Content.Load<SoundEffect>("Sound/MenuSounds/MenuHit");
        }

        public void initializeButtons()
        {
            int centerWidth = sharedVariables.GraphicsManager.PreferredBackBufferWidth / 2;
            int centerHeight = sharedVariables.GraphicsManager.PreferredBackBufferHeight / 2;

            backButton = new UserControls.Button(sharedVariables.Content, "Back", new Rectangle(centerWidth - texture.Width / 2, centerHeight + 200, texture.Width, texture.Height), texture);


            Controls.Clear();
            Controls.Add(backButton);
        }
        private void onButtonHover()
        {
            foreach (UserControls.Control btn in Controls)
            {
                if (btn.IsMouseOver)
                {
                    Rectangle buttonHoverRectangle = new Rectangle(btn.Rectangle.X, btn.Rectangle.Y, btn.Rectangle.Width, btn.Rectangle.Height);
                    if (!btn.HoverSoundPlayed && btn.IsMouseOver)
                    {
                        btn.HoverSoundPlayed = true;
                        hoverSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                    }
                }
                else
                {
                    btn.HoverSoundPlayed = false;
                }
            }

            if(backButton.IsLeftClicked)
            {
                backSound.Play(sharedVariables.SoundFxVolume, 0f, 0f);
                this.IsActive = false;
                game.MenuScreen.IsActive = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            foreach (UserControls.Control btn in Controls)
            {
                btn.Draw(spriteBatch);
                if (btn.IsMouseOver)
                {
                    spriteBatch.Draw(hoverTexture, new Vector2(btn.Rectangle.X, btn.Rectangle.Y), Color.White);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            presentMouse = Mouse.GetState();
            foreach (UserControls.Control control in Controls)
            {
                control.Update(presentMouse);
            }

            onButtonHover();

        }
    }
}
