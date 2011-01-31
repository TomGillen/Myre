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

        public override void Initialise(Renderer renderer)
        {
            renderables = renderer.Scene.FindManagers<IRenderable>();
            base.Initialise(renderer);
        }

        protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? output)
        {
            output = null;
        }

        protected internal override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
        {
            if (previousRenderTarget == null)
                return false;

            return true;
        }

        public override RenderTarget2D Draw(Renderer renderer)
        {
            foreach (var item in renderables)
                item.Draw(renderer);

            return null;
        }
    }
}
