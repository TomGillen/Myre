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

namespace Myre.Graphics
{
    public class EdgeDetectComponent
        : RendererComponent
    {
        private Material edgeDetect;
        private Quad quad;

        public EdgeDetectComponent(ContentManager content, GraphicsDevice device)
        {
            edgeDetect = new Material(content.Load<Effect>("EdgeDetect"));
            quad = new Quad(device);
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define inputs
            context.DefineInput("gbuffer_depth");
            context.DefineInput("gbuffer_normals");

            // define outputs
            context.DefineOutput("edges", isLeftSet: true, surfaceFormat: SurfaceFormat.Color);

            // define settings
            var settings = renderer.Settings;
            settings.Add("edge_normalthreshold", "Threshold used to decide between an edge and a non-edge by normal.", 0.7f);
            settings.Add("edge_depththreshold", "Threshold used to decide between an edge and a non-edge by depth.", 0.001f);
            settings.Add("edge_normalweight", "Weighting used for edges detected via normals in the output.", 0.15f);
            settings.Add("edge_depthweight", "Weighting used for edges detected via depth in the output.", 0.2f);

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var resolution = metadata.Get<Vector2>("resolution").Value;
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            var target = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.Color, DepthFormat.None, name: "edges");

            device.SetRenderTarget(target);
            device.BlendState = BlendState.Opaque;
            device.Clear(Color.Black);

            edgeDetect.Parameters["TexelSize"].SetValue(new Vector2(1f / width, 1f / height));
            quad.Draw(edgeDetect, metadata);

            Output("edges", target);
        }
    }
}