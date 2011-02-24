using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre.Debugging.Statistics;
using Myre.Entities;
using Myre.Entities.Services;
using Myre.Extensions;

namespace Myre.Graphics
{
    public class Renderer
        : Service
    {
        struct Resource
        {
            public string Name;
            public RenderTarget2D Target;
        }

        private Dictionary<string, RenderTarget2D> activeResources;
        private RendererMetadata data;
        private RendererSettings settings;
        private Queue<Texture2D> viewResults;

        private GraphicsDevice device;
        private Quad quad;
        //private Effect colourCorrection;
        private SpriteBatch spriteBatch;

        private RenderPlan plan;

        private Scene scene;
        private IEnumerable<View> views;

        public GraphicsDevice Device
        {
            get { return device; }
        }

        public Scene Scene
        {
            get { return scene; }
        }

        public RendererMetadata Data
        {
            get { return data; }
        }

        public RendererSettings Settings
        {
            get { return settings; }
        }

        public RenderPlan Plan
        {
            get { return plan; }
            set
            {
                plan = value;
                //plan.Apply();
            }
        }

        public Renderer(GraphicsDevice device, ContentManager content, Scene scene)
        {
            this.device = device;
            this.scene = scene;
            this.data = new RendererMetadata();
            this.settings = new RendererSettings(this);
            this.activeResources = new Dictionary<string, RenderTarget2D>();
            this.viewResults = new Queue<Texture2D>();
            this.quad = new Quad(device);
            //this.colourCorrection = content.Load<Effect>("Gamma");
            this.spriteBatch = new SpriteBatch(device);

            this.views = from manager in scene.FindManagers<View.Manager>()
                         from view in manager.Views
                         select view;
        }

        public override void Update(float elapsedTime)
        {
            data.Set<float>("timedelta", elapsedTime);
            base.Update(elapsedTime);
        }

        public override void Draw()
        {
#if PROFILE
            Statistic.Get("Graphics.Primitives").Value = 0;
            Statistic.Get("Graphics.Draws").Value = 0;
#endif

            foreach (var view in views)
            {
                view.SetMetadata(data);
                var output = Plan.Execute(this);

                activeResources.Clear();

                viewResults.Enqueue(output);
            }

            device.SetRenderTarget(null);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            //colourCorrection.Parameters["Resolution"].SetValue(data.Get<Vector2>("resolution").Value);
            foreach (var view in views)
            {
                var output = viewResults.Dequeue();
                var viewport = view.Viewport;

                //colourCorrection.Parameters["Texture"].SetValue(output);
                //quad.SetPosition(viewport.Bounds);
                //quad.Draw(colourCorrection);

                if (output.Format.IsFloatingPoint())
                    device.SamplerStates[0] = SamplerState.PointClamp;
                else
                    device.SamplerStates[0] = SamplerState.LinearClamp;

                spriteBatch.Draw(output, viewport.Bounds, Color.White);

                if (output is RenderTarget2D)
                    RenderTargetManager.RecycleTarget(output as RenderTarget2D);
            }
            spriteBatch.End();

            base.Draw();
        }

        public void SetResource(string name, RenderTarget2D target)
        {
            activeResources[name] = target;
            data.Set<Texture2D>(name, target);
        }

        internal RenderTarget2D GetResource(string name)
        {
            return activeResources[name];
        }

        public void FreeResource(string name)
        {
            RenderTarget2D target;
            if (activeResources.TryGetValue(name, out target))
            {
                RenderTargetManager.RecycleTarget(target);
                data.Set<Texture2D>(name, null);
                activeResources.Remove(name);
            }
        }
    }
}
