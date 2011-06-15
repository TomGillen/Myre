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
using Ninject;

namespace Myre.Graphics.Deferred
{
    public class AntiAliasComponent
        : RendererComponent
    {
        private Material edgeBlur;
        private Quad quad;
        private string inputResource;

        [Inject]
        public AntiAliasComponent(ContentManager content, GraphicsDevice device)
            : this(content, device, null)
        {
        }

        public AntiAliasComponent(ContentManager content, GraphicsDevice device, string inputResource)
        {
            this.edgeBlur = new Material(content.Load<Effect>("EdgeBlur"));
            this.quad = new Quad(device);
            this.inputResource = inputResource;
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define inputs
            if (inputResource == null)
                inputResource = context.SetRenderTargets[0].Name;
            
            context.DefineInput(inputResource);
            context.DefineInput("edges");

            // define outputs
            context.DefineOutput("antialiased", isLeftSet: true, surfaceFormat: SurfaceFormat.Color);
            
            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var resolution = metadata.Get<Vector2>("resolution").Value;
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            var target = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.Color, DepthFormat.None, name: "antialiased");

            device.SetRenderTarget(target);
            device.BlendState = BlendState.Opaque;
            device.Clear(Color.Black);

            edgeBlur.Parameters["Texture"].SetValue(GetResource(inputResource));
            edgeBlur.Parameters["TexelSize"].SetValue(new Vector2(1f / width, 1f / height));
            quad.Draw(edgeBlur, metadata);

            Output("antialiased", target);
        }
    }
}