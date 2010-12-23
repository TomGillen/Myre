using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Myre.Graphics
{
    public class RestoreDepthPhase
        : RendererComponent
    {
        private Quad quad;
        private Material restoreDepth;

        public bool ClearDepth { get; set; }

        public RestoreDepthPhase(GraphicsDevice device, ContentManager content)
        {
            this.quad = new Quad(device);
            this.restoreDepth = new Material(content.Load<Effect>("RestoreDepth").Clone());
            this.ClearDepth = true;
        }

        protected override void SpecifyResources(IList<Input> inputs, IList<Resource> outputs, out RenderTargetInfo? output)
        {
            inputs.Add(new Input() { Name = "gbuffer_depth" });
            output = null;
        }

        protected internal override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
        {
            if (previousRenderTarget == null)
                return false;

            if (previousRenderTarget.Value.DepthFormat == DepthFormat.None)
                return false;

            return true;
        }

        public override RenderTarget2D Draw(Renderer renderer)
        {
            // work arround for a bug in xna 4.0
            renderer.Device.SamplerStates[0] = SamplerState.LinearClamp;
            renderer.Device.SamplerStates[0] = SamplerState.PointClamp;

            if (ClearDepth)
                renderer.Device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1, 0);

            renderer.Device.DepthStencilState = DepthStencilState.Default;
            renderer.Device.BlendState = BlendState.Additive;
            quad.Draw(restoreDepth, renderer.Data);

            //var sb = new SpriteBatch(renderer.Device);
            //sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            //renderer.Device.SamplerStates[0] = SamplerState.PointClamp;
            //sb.Draw(renderer.Data.Get<Texture2D>("gbuffer_depth").Value, Vector2.Zero, Color.White);
            //sb.End();

            return null;
        }
    }
}
