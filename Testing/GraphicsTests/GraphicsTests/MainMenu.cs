﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.StateManagement;
using Myre.UI.Controls;
using Myre.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Reflection;
using Ninject;
using Myre.Entities;
using Myre.UI.Gestures;
using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.Input;
using Myre.UI.Text;
using Microsoft.Xna.Framework;
using Myre.Entities.Ninject;
using Ninject.Planning.Bindings.Resolvers;
using Myre.Debugging;

namespace GraphicsTests
{
    class MainMenu
        : Screen
    {
        private UserInterface ui;

        public MainMenu(Game game, InputActor player, CommandConsole console, GraphicsDevice device, ContentManager content, IServiceProvider services)
        {
            ui = new UserInterface(device);
            ui.Actors.Add(player);

            var tests = from type in Assembly.GetExecutingAssembly().GetTypes()
                        where typeof(TestScreen).IsAssignableFrom(type)
                        where !type.IsAbstract
                        select type;

            var menu = new Menu(ui.Root);
            menu.SetPoint(Points.BottomLeft, 50, -50);


            int index = 0;
            foreach (var test in tests)
            {
                index++;

                var testKernel = new StandardKernel(new EntityNinjectModule());
                testKernel.Components.Add<IBindingResolver, ConditionalBindingResolver>();
                testKernel.Bind<GraphicsDevice>().ToConstant(device);
                testKernel.Bind<ContentManager>().ToConstant(new ContentManager(services));
                testKernel.Bind<Game>().ToConstant(game);
                testKernel.Bind<CommandConsole>().ToConstant(console);
                testKernel.Bind<InputActor>().ToConstant(player);

                var instance = testKernel.Get(test) as TestScreen;

                var menuOption = new TextButton(menu, content.Load<SpriteFont>("Consolas"), instance.Name);
                menuOption.Highlight = Color.Red;
                menuOption.Gestures.Bind((gesture, time, input) => Manager.Push(instance), new MouseReleased(MouseButtons.Left), new KeyReleased(Keys.Enter));
            }

            var quit = new TextButton(menu, content.Load<SpriteFont>("Consolas"), "Exit");
            quit.Highlight = Color.Red;
            quit.Gestures.Bind((gesture, time, input) => game.Exit(), new MouseReleased(MouseButtons.Left), new KeyReleased(Keys.Enter));

            menu.Arrange(Justification.Left);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            ui.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            ui.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}