using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.PostProcessing;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Lighting
{
    public class Ssao
        : RendererComponent
    {
        private Material ssaoMaterial;
        private RenderTarget2D ssao;
        private Gaussian gaussian;
        private Quad quad;

        public Ssao(GraphicsDevice device, ContentManager content)
        {
            this.ssaoMaterial = new Material(content.Load<Effect>("SSAO"));
            this.ssaoMaterial.Parameters["Random"].SetValue(content.Load<Texture2D>("randomnormals"));
            this.gaussian = new Gaussian(device, content);
            this.quad = new Quad(device);
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define settings
            var settings = renderer.Settings;
            //settings.Add("ssao_enabled", "Determines if Screen Space Ambient Occlusion is enabled.", true);
            //settings.Add("ssao_halfres", "Determines if SSAO will run at full of half screen resolution.", true);
            settings.Add("ssao_radius", "SSAO sample radius", 6f);
            settings.Add("ssao_intensity", "SSAO intensity", 20f);
            settings.Add("ssao_scale", "Scales distance between occluders and occludee.", 1f);
            //settings.Add("ssao_detailradius", "SSAO sample radius", 2.3f);
            //settings.Add("ssao_detailintensity", "SSAO intensity", 15f);
            //settings.Add("ssao_detailscale", "Scales distance between occluders and occludee.", 1.5f);
            settings.Add("ssao_blur", "The amount to blur SSAO.", 1f);
            settings.Add("ssao_radiosityintensity", "The intensity of local radiosity colour transfer.", 0.5f);

            // define inputs
            context.DefineInput("gbuffer_depth_downsample");
            context.DefineInput("gbuffer_normals");
            context.DefineInput("gbuffer_diffuse");

            // define outputs
            context.DefineOutput("ssao");

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var resolution = renderer.Data.Get<Vector2>("resolution").Value;

            if (renderer.Data.Get<float>("ssao_radiosityintensity").Value > 0)
                ssaoMaterial.CurrentTechnique = ssaoMaterial.Techniques["SSGI"];
            else
                ssaoMaterial.CurrentTechnique = ssaoMaterial.Techniques["SSAO"];

            var unblured = RenderTargetManager.GetTarget(renderer.Device, (int)resolution.X, (int)resolution.Y, surfaceFormat: SurfaceFormat.HalfVector4, name: "ssao unblurred");//, SurfaceFormat.HalfVector4);
            renderer.Device.SetRenderTarget(unblured);
            renderer.Device.Clear(Color.Transparent);
            renderer.Device.BlendState = BlendState.Opaque;
            quad.Draw(ssaoMaterial, renderer.Data);

            //var blurSigma = renderer.Data.Get<float>("ssao_blur").Value;
            //if (blurSigma != 0)
            //{
            //    var blured = RenderTargetManager.GetTarget(renderer.Device, (int)resolution.X, (int)resolution.Y, name: "ssao blurred");//, SurfaceFormat.HalfVector4);
            //    gaussian.Blur(unblured, blured, blurSigma);
            //    ssao = blured;
            //    RenderTargetManager.RecycleTarget(unblured);
            //}
            //else
                ssao = unblured;

            Output("ssao", ssao);
        }
    }
}
