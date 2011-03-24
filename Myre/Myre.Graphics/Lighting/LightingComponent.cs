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
    public class LightingComponent
       : RendererComponent
    {
        //protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
        //{
        //    inputs.Add(new Input() { Name = "gbuffer_depth" });
        //    inputs.Add(new Input() { Name = "gbuffer_normals" });
        //    inputs.Add(new Input() { Name = "gbuffer_diffuse" });
        //    inputs.Add(new Input() { Name = "gbuffer_depth_downsample" });

        //    outputs.Add(new Resource()
        //    {
        //        Name = "lightbuffer",
        //        IsLeftSet = true,
        //        Finaliser = null
        //    });

        //    outputs.Add(new Resource()
        //    {
        //        Name = "previouslightbuffer",
        //        IsLeftSet = false,
        //        Finaliser = null
        //    });

        //    outputs.Add(new Resource() { Name = "ssao" });

        //    outputTarget = new RenderTargetInfo()
        //    {
        //        SurfaceFormat = SurfaceFormat.HdrBlendable,
        //        DepthFormat = DepthFormat.Depth24Stencil8
        //    };
        //}

        //protected internal override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
        //{
        //    return true;
        //}

        Quad quad;
        Material restoreDepth;
        Material markGeometry;
        DepthStencilState markGeometryState;
        Comparison<ILightProvider> lightComparison;
        List<ILightProvider> lights;
        RenderTarget2D lightBuffer;
        RenderTarget2D previousTarget;

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

        public LightingComponent(
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

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // create debug dettings
            var settings = renderer.Settings;

            settings.Add("lighting_attenuationscale", "Scales the rate at which lights attenuate over distance.", 100);
            settings.Add("lighting_threshold", "The fraction of the average scene luminance at which the rage of a light is cut off.", 0.05f);
            settings.Add("debuglights", "Shows light debug information", false);

            // define inputs
            context.DefineInput("gbuffer_depth");
            context.DefineInput("gbuffer_normals");
            context.DefineInput("gbuffer_diffuse");
            context.DefineInput("gbuffer_depth_downsample");

            if (context.AvailableResources.Any(r => r.Name == "ssao"))
                context.DefineInput("ssao");

            // define outputs
            context.DefineOutput("lightbuffer", isLeftSet:true, finaliser: (r, t) => {}, surfaceFormat:SurfaceFormat.HdrBlendable, depthFormat:DepthFormat.Depth24Stencil8);
            context.DefineOutput("previouslightbuffer", isLeftSet:false, surfaceFormat:SurfaceFormat.HdrBlendable, depthFormat:DepthFormat.Depth24Stencil8);

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
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

            SwapBuffers(renderer, device, width, height);
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
            
            Output("lightbuffer", lightBuffer);
            Output("previouslightbuffer", previousTarget);
        }

        private void SwapBuffers(Renderer renderer, GraphicsDevice device, int width, int height)
        {
            if (previousTarget != null)
            {
                //RenderTargetManager.RecycleTarget(previousTarget);
                previousTarget = lightBuffer;
            }
            else
            {
                previousTarget = RenderTargetManager.GetTarget(renderer.Device, 1, 1, name:"previous light buffer");
                renderer.Device.SetRenderTarget(previousTarget);
                renderer.Device.Clear(Color.Transparent);
            }

            lightBuffer = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8, 0, false, RenderTargetUsage.DiscardContents, name:"light buffer");
        }
    }
}
