using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Microsoft.Xna.Framework;
using Myre.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Materials;
using Ninject;

namespace Myre.Graphics.Lighting
{
    [DefaultManager(typeof(Manager))]
    public class AmbientLight
        : Behaviour
    {
        private Property<Vector3> skyColour;
        private Property<Vector3> groundColour;
        private Property<Vector3> up;

        public Vector3 SkyColour
        {
            get { return skyColour.Value; }
            set { skyColour.Value = value; }
        }

        public Vector3 GroundColour
        {
            get { return groundColour.Value; }
            set { groundColour.Value = value; }
        }

        public Vector3 Up
        {
            get { return up.Value; }
            set { up.Value = value; }
        }

        public override void Initialise(Entity.InitialisationContext context)
        {
            this.skyColour = context.GetOrCreateProperty<Vector3>("sky_colour");
            this.groundColour = context.GetOrCreateProperty<Vector3>("ground_colour");
            this.up = context.GetOrCreateProperty<Vector3>("up");
            base.Initialise(context);
        }

        /*
        [Inject, Name("skycolour")]
        public Property<Vector3> SkyColour { get; set; }

        [Inject, Name("groundcolour")]
        public Property<Vector3> GroundColour { get; set; }

        [Inject, Name("up")]
        public Property<Vector3> Up { get; set; }
        */

        public class Manager
            : BehaviourManager<AmbientLight>, ILightProvider
        {
            private Material lightingMaterial;
            private Material ssaoMaterial;
            private RenderTarget2D ssao;

            private Quad quad;

            public bool ModifiesStencil
            {
                get { return false; }
            }

            public Manager(
                ContentManager content,
                GraphicsDevice device,
                [SceneService] Renderer renderer)
            {                
                lightingMaterial = new Material(content.Load<Effect>("AmbientLight"));
                ssaoMaterial = new Material(content.Load<Effect>("SSAO"));
                //ssaoMaterial.Parameters["Offsets"].SetValue(RandomVectors(16));
                ssaoMaterial.Parameters["Random"].SetValue(RandomTexture(device));//content.Load<Texture2D>("randomnormals"));

                quad = new Quad(device);
            }

            private Texture2D RandomTexture(GraphicsDevice device)
            {
                var tex = new Texture2D(device, 250, 250);
                var data = new Color[tex.Width * tex.Height];

                Random rng = new Random();
                for (int i = 0; i < data.Length; i++)
                {
                    var vector = new Vector2((float)rng.NextDouble() * 2 - 1, (float)rng.NextDouble() * 2 - 1);
                    vector.Normalize();
                    vector *= MathHelper.Lerp(0.5f, 1f, (float)rng.NextDouble());

                    vector = (vector + Vector2.One) / 2;
                    data[i] = new Color(vector.X, vector.Y, 0, 1);
                }

                tex.SetData(data);
                return tex;
            }

            private Vector3[] RandomVectors(int samples)
            {
                var vectors = new Vector3[samples];

                var dlong = Math.PI * (3 - Math.Sqrt(5));
                var dz = 2.0 / samples;
                var longitude = 0.0;
                var z = 1 - dz / 2;
                for (int i = 0; i < samples; i++)
			    {
                    var r = Math.Sqrt(1 - z * z);
			        vectors[i] = new Vector3((float)(Math.Cos(longitude) * r),
                                             (float)(Math.Sin(longitude) * r),
                                             (float)z);
                    z -= dz;
                    longitude += dlong;
			    }

                return vectors;
            }

            public bool PrepareDraw(Renderer renderer)
            {
                if (renderer.Data.Get<bool>("ssao_enabled").Value)
                    DrawSsao(renderer);

                return true;
            }

            public void Draw(Renderer renderer)
            {
                var metadata = renderer.Data;
                var view = metadata.Get<Matrix>("view").Value;

                if (ssao != null)
                    lightingMaterial.CurrentTechnique = lightingMaterial.Techniques["AmbientSSAO"];
                else
                    lightingMaterial.CurrentTechnique = lightingMaterial.Techniques["Ambient"];

                lightingMaterial.Parameters["SSAO"].SetValue(ssao);

                foreach (var light in Behaviours)
                {
                    lightingMaterial.Parameters["Up"].SetValue(Vector3.TransformNormal(light.Up, view));
                    lightingMaterial.Parameters["SkyColour"].SetValue(light.SkyColour);
                    lightingMaterial.Parameters["GroundColour"].SetValue(light.GroundColour);

                    quad.Draw(lightingMaterial, metadata);
                }

                if (ssao != null)
                {
                    RenderTargetManager.RecycleTarget(ssao);
                    ssao = null;
                }
            }

            public void DrawDebug(Renderer renderer)
            {
            }

            private void DrawSsao(Renderer renderer)
            {
                var resolution = renderer.Data.Get<Vector2>("resolution");
                var fullRes = resolution.Value;

                Vector2 ssaoRes;
                if (renderer.Data.Get<bool>("ssao_halfres").Value)
                    ssaoRes = fullRes / 2;
                else
                    ssaoRes = fullRes;

                ssao = RenderTargetManager.GetTarget(renderer.Device, (int)ssaoRes.X, (int)ssaoRes.Y);
                renderer.Device.SetRenderTarget(ssao);
                resolution.Value = ssaoRes;
                quad.Draw(ssaoMaterial, renderer.Data);
                resolution.Value = fullRes;
            }
        }
    }
}
