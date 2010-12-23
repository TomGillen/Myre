using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Myre.Graphics
{
    public class CreateTargetComponent
        : RendererComponent
    {
        private static int counter;

        private string name;
        private RenderTargetInfo targetInfo;

        public CreateTargetComponent(RenderTargetInfo targetInfo, string resourceName = null)
        {
            this.targetInfo = targetInfo;

            counter = (counter + 1) % (int.MaxValue - 1);
            this.name = resourceName ?? string.Format("anonymous-{0}-{1}", counter, targetInfo.GetHashCode());
        }

        protected override void SpecifyResources(IList<Input> inputResources, IList<RendererComponent.Resource> outputResources, out RenderTargetInfo? outputTarget)
        {
            outputResources.Add(new Resource() { Name = name });
            outputTarget = targetInfo;
        }

        protected internal override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
        {
            return true;
        }

        public override RenderTarget2D Draw(Renderer renderer)
        {
            var info = targetInfo;
            if (info.Width == 0 || info.Height == 0)
            {
                var resolution = renderer.Data.Get<Vector2>("resolution").Value;
                info.Width = (int)resolution.X;
                info.Height = (int)resolution.Y;
            }

            var target = RenderTargetManager.GetTarget(renderer.Device, info);
            renderer.Device.SetRenderTarget(target);
            renderer.Device.Clear(Color.Black);

            return target;
        }
    }
}
