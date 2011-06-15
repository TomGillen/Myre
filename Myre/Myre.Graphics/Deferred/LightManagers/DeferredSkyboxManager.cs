using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Graphics.Lighting;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Deferred.LightManagers
{
    public class DeferredSkyboxManager
            : BehaviourManager<Skybox>, IIndirectLight
    {
        Model model;
        Effect skyboxEffect;

        public DeferredSkyboxManager(
            GraphicsDevice device,
            ContentManager content)
        {
            skyboxEffect = content.Load<Effect>("Skybox");
            model = content.Load<Model>("SkyboxModel");
        }

        public void Prepare(Renderer renderer)
        {
        }

        public void Draw(Renderer renderer)
        {
            var device = renderer.Device;

            //var previousDepthState = device.DepthStencilState;
            //device.DepthStencilState = LightingComponent.CullGeometry;
            ////device.DepthStencilState = DepthStencilState.None;

            //var previousRasterState = device.RasterizerState;
            //device.RasterizerState = RasterizerState.CullNone;

            ////device.SetVertexBuffer(vertices);
            ////device.Indices = indices;

            //var part = model.Meshes[0].MeshParts[0];
            //device.SetVertexBuffer(part.VertexBuffer);
            //device.Indices = part.IndexBuffer;

            //skyboxEffect.Parameters["View"].SetValue(renderer.Data.Get<Matrix>("view").Value);
            //skyboxEffect.Parameters["Projection"].SetValue(renderer.Data.Get<Matrix>("projection").Value);

            //for (int i = 0; i < Behaviours.Count; i++)
            //{
            //    var light = Behaviours[i];
            //    skyboxEffect.Parameters["EnvironmentMap"].SetValue(light.Texture);
            //    skyboxEffect.Parameters["Brightness"].SetValue(light.Brightness);

            //    skyboxEffect.CurrentTechnique = light.GammaCorrect ? skyboxEffect.Techniques["SkyboxGammaCorrect"] : skyboxEffect.Techniques["Skybox"];

            //    foreach (var pass in skyboxEffect.CurrentTechnique.Passes)
            //    {
            //        pass.Apply();
            //        //device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 12);
            //        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
            //    }
            //}

            //device.DepthStencilState = previousDepthState;
            //device.RasterizerState = previousRasterState;
        }
    }
}
