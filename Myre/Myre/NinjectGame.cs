using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Ninject;
using Ninject.Modules;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Myre
{
    public abstract class NinjectGame
        : Game
    {
        public IKernel Kernel { get; set; }

        public NinjectGame(params NinjectModule[] modules)
            : base()
        {
            Kernel = new StandardKernel(modules);

            BindGameToKernel(this, Kernel, true, true, true);
        }

        public static void BindGameToKernel(NinjectGame game, IKernel kernel, bool bindGraphics, bool bindContent, bool bindServiceContainer)
        {
            // bind the game to a singleton instance
            var thisType = game.GetType();
            kernel.Bind(thisType).ToConstant(game);
            kernel.Bind<NinjectGame>().ToConstant(game);
            kernel.Bind<Game>().ToConstant(game);

            // bind the graphics device
            if (bindGraphics)
                kernel.Bind<GraphicsDevice>().ToMethod(context => context.Kernel.Get<Game>().GraphicsDevice).InSingletonScope();

            // bind the content manager
            if (bindContent)
                kernel.Bind<ContentManager>().ToMethod(context => context.Kernel.Get<Game>().Content).InSingletonScope();

            // bind services
            if (bindServiceContainer)
            {
                kernel.Bind<GameServiceContainer>().ToMethod(context => context.Kernel.Get<Game>().Services).InSingletonScope();
                kernel.Bind<IServiceProvider>().ToMethod(context => context.Kernel.Get<Game>().Services).InSingletonScope();
            }
        }
    }
}
