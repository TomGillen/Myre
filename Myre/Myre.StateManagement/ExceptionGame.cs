using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Content;

namespace Myre.StateManagement
{
    class ExceptionGame : Game
    {
        public const string ErrorTitle = "Unexpected Error";
        public const string ErrorMessage = "The game had an unexpected error and had to shut down.";

        public static readonly string[] ErrorButtons = new[]
        {
            "Exit Game"
        };

        private readonly Exception exception;
        private bool shownMessage;

        private SpriteBatch batch;

        public ExceptionGame(Exception e)
        {
            new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };
            exception = e;
            Components.Add(new GamerServicesComponent(this));
        }

        protected override void LoadContent()
        {
            batch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            try
            {
                if (!shownMessage)
                {
                    if (!Guide.IsVisible)
                    {
                        Guide.BeginShowMessageBox(
                            PlayerIndex.One,
                            ErrorTitle,
                            ErrorMessage,
                            ErrorButtons,
                            0,
                            MessageBoxIcon.Error,
                            result => Exit(),
                            null);
                        shownMessage = true;
                    }
                }
            }
            catch { }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
        }
    }
}
