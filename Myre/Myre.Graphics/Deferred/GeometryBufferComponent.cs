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
using Myre.Graphics.Geometry;

namespace Myre.Graphics.Deferred
{
    public class GeometryBufferComponent
        : RendererComponent
    {
        private Resample scale;
        private Material clear;
        private Quad quad;

        public GeometryBufferComponent(GraphicsDevice device)
        {
            clear = new Material(Content.Load<Effect>("ClearGBuffer"));
            scale = new Resample(device);
            quad = new Quad(device);
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define outputs
            context.DefineOutput("gbuffer_depth", isLeftSet: false, surfaceFormat: SurfaceFormat.Single, depthFormat: DepthFormat.Depth24Stencil8);
            context.DefineOutput("gbuffer_normals", isLeftSet: false, surfaceFormat: SurfaceFormat.Rgba1010102);
            context.DefineOutput("gbuffer_diffuse", isLeftSet: false, surfaceFormat: SurfaceFormat.Color);
            context.DefineOutput("gbuffer_depth_downsample", isLeftSet: true, surfaceFormat: SurfaceFormat.Single);

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var resolution = metadata.Get<Vector2>("resolution").Value;
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            var depth = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, name:"depth");
            var normals = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.Rgba1010102, name:"normals");
            var diffuse = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.Color, name:"diffuse");

            device.SetRenderTargets(depth, normals, diffuse);
            device.BlendState = BlendState.Opaque;
            device.Clear(Color.Black);

            device.DepthStencilState = DepthStencilState.None;
            quad.Draw(clear, metadata);
            device.DepthStencilState = DepthStencilState.Default;

            device.BlendState = BlendState.Opaque;

            foreach (var geometryProvider in renderer.Scene.FindManagers<IGeometryProvider>())
                geometryProvider.Draw("gbuffer", metadata);

            Output("gbuffer_depth", depth);
            Output("gbuffer_normals", normals);
            Output("gbuffer_diffuse", diffuse);

            DownsampleDepth(renderer, depth);
        }

        private void DownsampleDepth(Renderer renderer, RenderTarget2D depth)
        {
            var downsampled = RenderTargetManager.GetTarget(renderer.Device, depth.Width / 2, depth.Height / 2, SurfaceFormat.Single, name:"downsample depth");
            scale.Scale(depth, downsampled);
            Output("gbuffer_depth_downsample", downsampled);
        }
    }
}