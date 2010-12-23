using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Materials;
using Myre.Collections;
using Myre.Graphics.PostProcessing;

namespace Myre.Graphics
{
    public class GeometryBufferPhase
        : RendererComponent
    {
        protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? output)
        {
            outputs.Add(new Resource() { Name = "gbuffer_depth" });
            outputs.Add(new Resource() { Name = "gbuffer_normals" });
            outputs.Add(new Resource() { Name = "gbuffer_diffuse" });
            outputs.Add(new Resource() { Name = "gbuffer_depth_downsample", IsLeftSet = true });

            output = new RenderTargetInfo()
            {
                SurfaceFormat = SurfaceFormat.Rg32,
                DepthFormat = DepthFormat.Depth24Stencil8
            };
        }

        protected internal override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
        {
            return true;
        }

        private Resample scale;
        private Material clear;
        private Quad quad;

        public GeometryBufferPhase(ContentManager content, GraphicsDevice device)
        {
            clear = new Material(content.Load<Effect>("ClearGBuffer"));
            scale = new Resample(device, content);
            quad = new Quad(device);
        }

        public override RenderTarget2D Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var resolution = metadata.Get<Vector2>("resolution").Value;
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            var depth = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            var normals = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.Rgba1010102);
            var diffuse = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.Color);

            device.SetRenderTargets(depth, normals, diffuse);

            renderer.SetResource("gbuffer_depth", depth);
            renderer.SetResource("gbuffer_normals", normals);
            renderer.SetResource("gbuffer_diffuse", diffuse);

            renderer.Device.BlendState = BlendState.Opaque;

            renderer.Device.Clear(Color.Black);

            device.DepthStencilState = DepthStencilState.None;
            quad.Draw(clear, metadata);
            device.DepthStencilState = DepthStencilState.Default;

            for (int i = 0; i < renderer.Geometry.Count; i++)
            {
                renderer.Geometry[i].Draw("gbuffer", metadata);
            }

            return DownsampleDepth(renderer, depth);
        }

        private RenderTarget2D DownsampleDepth(Renderer renderer, RenderTarget2D depth)
        {
            var downsampled = RenderTargetManager.GetTarget(renderer.Device, depth.Width / 2, depth.Height / 2, SurfaceFormat.Rg32);
            scale.Scale(depth, downsampled);
            renderer.SetResource("gbuffer_depth_downsample", downsampled);

            return downsampled;
        }
    }
}