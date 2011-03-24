using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics
{
    /// <summary>
    /// A renderer component which draws IRenderable objects.
    /// </summary>
    public class RenderableComponent
        : RendererComponent
    {
        private ReadOnlyCollection<IRenderable> renderables;

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            foreach (var resource in context.SetRenderTargets)
            {
                context.DefineInput(resource.Name);
                context.DefineOutput(resource);
            }

            renderables = renderer.Scene.FindManagers<IRenderable>();
            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            foreach (var item in renderables)
                item.Draw(renderer);
        }
    }
}
