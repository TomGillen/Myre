using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.StateManagement;
using Myre.UI;
using Microsoft.Xna.Framework;
using Myre.UI.Controls;
using Microsoft.Xna.Framework.Content;
using Ninject;
using Microsoft.Xna.Framework.Graphics;
using Myre.UI.Text;
using Myre.UI.Gestures;
using Microsoft.Xna.Framework.Input;
using Myre.UI.InputDevices;

namespace GraphicsTests
{
    abstract class TestScreen
        : Screen
    {
        private ContentManager content;
        private InputActor actor;

        public string Name { get; private set; }
        public UserInterface UI { get; private set; }

        public TestScreen(string name, IKernel kernel)
        {
            Name = name;

            content = kernel.Get<ContentManager>();
            content.RootDirectory = "Content";

            UI = kernel.Get<UserInterface>();
            UI.Root.Gestures.Bind((gesture, time, device) => Manager.Pop(), new KeyReleased(Keys.Escape));

            actor = kernel.Get<InputActor>();
            UI.Actors.Add(actor);

            var title = new Label(UI.Root, content.Load<SpriteFont>("Consolas"));
            title.Text = Name;
            title.Justification = Justification.Centre;
            title.SetPoint(Points.Top, Int2D.Zero);
        }

        public override void OnShown()
        {
            actor.Focus(UI.Root);
            base.OnShown();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var actor in UI.Actors)
                actor.Update(gameTime);

            UI.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            UI.Draw(gameTime);
            base.Draw(gameTime);
        }

        public override void OnHidden()
        {
            base.OnHidden();
            content.Unload();
        }
    }
}
