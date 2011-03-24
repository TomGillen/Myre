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
using Myre.Graphics.PostProcessing;

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

        public override void CreateProperties(Entity.InitialisationContext context)
        {
            this.skyColour = context.CreateProperty<Vector3>("sky_colour");
            this.groundColour = context.CreateProperty<Vector3>("ground_colour");
            this.up = context.CreateProperty<Vector3>("up");

            base.CreateProperties(context);
        }

        public class Manager
            : BehaviourManager<AmbientLight>, ILightProvider
        {
            private Material lightingMaterial;
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
                quad = new Quad(device);
            }

            public bool PrepareDraw(Renderer renderer)
            {
                return true;
            }

            public void Draw(Renderer renderer)
            {
                var metadata = renderer.Data;
                var view = metadata.Get<Matrix>("view").Value;
                var ssao = metadata.Get<Texture2D>("ssao").Value;

                if (ssao != null)
                    lightingMaterial.CurrentTechnique = lightingMaterial.Techniques["AmbientSSAO"];
                else
                    lightingMaterial.CurrentTechnique = lightingMaterial.Techniques["Ambient"];

                foreach (var light in Behaviours)
                {
                    lightingMaterial.Parameters["Up"].SetValue(Vector3.TransformNormal(light.Up, view));
                    lightingMaterial.Parameters["SkyColour"].SetValue(light.SkyColour);
                    lightingMaterial.Parameters["GroundColour"].SetValue(light.GroundColour);

                    quad.Draw(lightingMaterial, metadata);
                }
            }

            public void DrawDebug(Renderer renderer)
            {
            }
        }
    }
}
