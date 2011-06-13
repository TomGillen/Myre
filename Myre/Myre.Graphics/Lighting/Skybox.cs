using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Ninject;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Lighting
{
    [DefaultManager(typeof(Manager))]
    public class Skybox
        : Behaviour
    {
        private Property<TextureCube> texture;
        private Property<float> brightness;
        private Property<bool> gammaCorrect;

        public TextureCube Texture
        {
            get { return texture.Value; }
            set { texture.Value = value; }
        }

        public float Brightness
        {
            get { return brightness.Value; }
            set { brightness.Value = value; }
        }

        public bool GammaCorrect
        {
            get { return gammaCorrect.Value; }
            set { gammaCorrect.Value = value; }
        }

        public override void CreateProperties(Entity.InitialisationContext context)
        {
            this.texture = context.CreateProperty<TextureCube>("texture");
            this.brightness = context.CreateProperty<float>("brightness");
            this.gammaCorrect = context.CreateProperty<bool>("gamma_correct");

            base.CreateProperties(context);
        }

        /*
        [Inject, Name("texture")]
        public Property<TextureCube> Texture { get; set; }

        [Inject, Name("brightness")]
        public Property<float> Brightness { get; set; }

        [Inject, Name("gammacorrect")]
        public Property<bool> GammaCorrect { get; set; }
        */


        public class Manager
            : BehaviourManager<Skybox>, ILightProvider
        {
            Model model;
            Effect skyboxEffect;
            //VertexBuffer vertices;
            //IndexBuffer indices;

            public Manager(
                GraphicsDevice device, 
                ContentManager content)
            {
                //var declaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0));

                //vertices = new VertexBuffer(device, declaration, 8, BufferUsage.None);
                //vertices.SetData<Vector3>(new Vector3[]
                //{
                //    new Vector3(-1, 1, 1),
                //    new Vector3(1, 1, 1),
                //    new Vector3(1, -1, 1),
                //    new Vector3(-1, -1, 1),        
                //    new Vector3(1, 1, -1),
                //    new Vector3(-1, 1, -1),
                //    new Vector3(-1, -1, -1),
                //    new Vector3(1, -1,- 1)
                //});

                //indices = new IndexBuffer(device, typeof(short), 36, BufferUsage.None);
                //indices.SetData<short>(new short[]
                //{
                //    0, 1, 2, 2, 3, 0,   // Front       
                //    1, 4, 7, 7, 2, 1,   // Right
                //    4, 5, 6, 6, 7, 4,   // Back
                //    5, 0, 3, 3, 6, 5,   // Left
                //    5, 4, 1, 1, 0, 5,   // Top
                //    3, 2, 7, 7, 6, 3    // Bottom
                //});

                skyboxEffect = content.Load<Effect>("Skybox");
                model = content.Load<Model>("SkyboxModel");
            }

            public bool ModifiesStencil
            {
                get { return false; }
            }

            public bool PrepareDraw(Renderer renderer)
            {
                return false;
            }

            public void Draw(Renderer renderer)
            {
                var device = renderer.Device;

                var previousDepthState = device.DepthStencilState;
                device.DepthStencilState = LightingComponent.CullGeometry;
                //device.DepthStencilState = DepthStencilState.None;

                var previousRasterState = device.RasterizerState;
                device.RasterizerState = RasterizerState.CullNone;

                //device.SetVertexBuffer(vertices);
                //device.Indices = indices;

                var part = model.Meshes[0].MeshParts[0];
                device.SetVertexBuffer(part.VertexBuffer);
                device.Indices = part.IndexBuffer;

                skyboxEffect.Parameters["View"].SetValue(renderer.Data.Get<Matrix>("view").Value);
                skyboxEffect.Parameters["Projection"].SetValue(renderer.Data.Get<Matrix>("projection").Value);

                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var light = Behaviours[i];
                    skyboxEffect.Parameters["EnvironmentMap"].SetValue(light.Texture);
                    skyboxEffect.Parameters["Brightness"].SetValue(light.Brightness);

                    skyboxEffect.CurrentTechnique = light.GammaCorrect ? skyboxEffect.Techniques["SkyboxGammaCorrect"] : skyboxEffect.Techniques["Skybox"];

                    foreach (var pass in skyboxEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        //device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 12);
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
                }

                device.DepthStencilState = previousDepthState;
                device.RasterizerState = previousRasterState;
            }

            public void DrawDebug(Renderer renderer)
            {
            }
        }
    }
}