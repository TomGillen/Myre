using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Content;
using System.IO;
using Myre.Extensions;

namespace Myre.Graphics.Lighting
{
    public class LightingPhase
       : RendererComponent
    {
        protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
        {
            inputs.Add(new Input() { Name = "gbuffer_depth" });
            inputs.Add(new Input() { Name = "gbuffer_normals" });
            inputs.Add(new Input() { Name = "gbuffer_diffuse" });
            inputs.Add(new Input() { Name = "gbuffer_depth_downsample" });

            outputs.Add(new Resource()
            {
                Name = "lightbuffer",
                IsLeftSet = true,
                Finaliser = (renderer, output) =>
                    {
                        var previous = renderer.Data.Get<Texture2D>("previouslightbuffer");
                        if (previous.Value != null)
                            RenderTargetManager.RecycleTarget(previous.Value as RenderTarget2D);
                        previous.Value = renderer.Data.Get<Texture2D>("lightbuffer").Value;
                    }
            });

            outputTarget = new RenderTargetInfo()
            {
                SurfaceFormat = SurfaceFormat.HdrBlendable,
                DepthFormat = DepthFormat.Depth24Stencil8
            };
        }

        protected internal override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
        {
            return true;
        }

        Quad quad;
        Material restoreDepth;
        Material markGeometry;
        DepthStencilState markGeometryState;
        Comparison<ILightProvider> lightComparison;
        List<ILightProvider> lights;

        public static readonly DepthStencilState CullGeometry = new DepthStencilState()
        {
            DepthBufferEnable = false,
            StencilEnable = true,
            ReferenceStencil = 0,
            StencilFunction = CompareFunction.Equal
        };

        public static readonly DepthStencilState CullNonGeometry = new DepthStencilState()
        {
            DepthBufferEnable = false,
            StencilEnable = true,
            ReferenceStencil = 0,
            StencilFunction = CompareFunction.NotEqual
        };

        public LightingPhase(
            GraphicsDevice device,
            ContentManager content)
        {
            quad = new Quad(device);
            quad.SetPosition(depth: 0.99999f);

            restoreDepth = new Material(content.Load<Effect>("RestoreDepth"), null);
            markGeometry = new Material(content.Load<Effect>("MarkGeometry"), null);

            markGeometryState = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                StencilEnable = true,
                StencilDepthBufferFail = StencilOperation.Replace,
                ReferenceStencil = 1
            };

            lightComparison = (a, b) => (a.ModifiesStencil ? 1 : 0).CompareTo(b.ModifiesStencil ? 1 : 0);
            lights = new List<ILightProvider>();
        }

        public override void Initialise(Renderer renderer)
        {
            var settings = renderer.Settings;

            settings.Add("lighting_attenuationscale", "Scales the rate at which lights attenuate over distance.", 100);
            settings.Add("lighting_threshold", "The fraction of the average scene luminance at which the rage of a light is cut off.", 0.05f);

            settings.Add("ssao_enabled", "Determines if Screen Space Ambient Occlusion is enabled.", true);
            settings.Add("ssao_halfres", "Determines if SSAO will run at full of half screen resolution.", true);
            settings.Add("ssao_radius", "SSAO sample radius", 6f);
            settings.Add("ssao_intensity", "SSAO intensity", 20f);
            settings.Add("ssao_scale", "Scales distance between occluders and occludee.", 1f);
            settings.Add("ssao_detailradius", "SSAO sample radius", 2.3f);
            settings.Add("ssao_detailintensity", "SSAO intensity", 15f);
            settings.Add("ssao_detailscale", "Scales distance between occluders and occludee.", 1.5f);

            settings.Add("debuglights", "Shows light debug information", false);


            //settings.Add("ssao_bias", "Controls the width of the occlusion cone considered by the occludee.", 0.045f);
            //settings.Add("ssao_selfocclusion", "Controls how much self occlusion is simulated.", 0f);
                        
            base.Initialise(renderer);
        }

        public override RenderTarget2D Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            lights.AddRange(renderer.Scene.FindManagers<ILightProvider>());
            lights.Sort(lightComparison);

            for (int i = 0; i < lights.Count; i++)
                lights[i].PrepareDraw(renderer);

            var resolution = metadata.Get<Vector2>("resolution").Value;
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            var lightBuffer = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8, 0, false, RenderTargetUsage.DiscardContents);

            device.SetRenderTarget(lightBuffer);

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.Clear(Color.Black);

            // work arround for a bug in xna 4.0
            renderer.Device.SamplerStates[0] = SamplerState.LinearClamp;
            renderer.Device.SamplerStates[0] = SamplerState.PointClamp;

            quad.Draw(restoreDepth, metadata);

            device.DepthStencilState = markGeometryState;
            quad.Draw(markGeometry, metadata);

            device.DepthStencilState = CullNonGeometry;
            device.BlendState = BlendState.Additive;

            for (int i = 0; i < lights.Count; i++)
                lights[i].Draw(renderer);

            if (metadata.Get<bool>("debuglights").Value)
            {
                for (int i = 0; i < lights.Count; i++)
                    lights[i].DrawDebug(renderer);
            }

            lights.Clear();

            renderer.SetResource("lightbuffer", lightBuffer);
            return lightBuffer;
        }
    }
}
