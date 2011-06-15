using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Graphics.Lighting;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Deferred.LightManagers
{
    public class DeferredPointLightManager
            : BehaviourManager<PointLight>, IDirectLight
    {
        private Material geometryLightingMaterial;
        private Material quadLightingMaterial;
        private Material nothingMaterial;
        private Quad quad;
        private Model geometry;

        private List<PointLight> touchesNearPlane;
        private List<PointLight> touchesFarPlane;
        private List<PointLight> touchesBothPlanes;
        private List<PointLight> touchesNeitherPlane;

        private DepthStencilState depthGreater;
        private BlendState colourWriteDisable;
        private DepthStencilState stencilWritePass;
        private DepthStencilState stencilCheckPass;

        public DeferredPointLightManager(
            ContentManager content,
            GraphicsDevice device)
        {
            var effect = content.Load<Effect>("PointLight");
            geometryLightingMaterial = new Material(effect.Clone(), "Geometry");
            quadLightingMaterial = new Material(effect.Clone(), "Quad");


            effect = content.Load<Effect>("Nothing");
            nothingMaterial = new Material(effect, null);

            geometry = content.Load<Model>("sphere");

            quad = new Quad(device);

            touchesNearPlane = new List<PointLight>();
            touchesFarPlane = new List<PointLight>();
            touchesBothPlanes = new List<PointLight>();
            touchesNeitherPlane = new List<PointLight>();

            depthGreater = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.GreaterEqual
            };

            stencilWritePass = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.LessEqual,
                StencilEnable = true,
                TwoSidedStencilMode = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Increment,
                CounterClockwiseStencilPass = StencilOperation.Decrement
            };

            stencilCheckPass = new DepthStencilState()
            {
                DepthBufferEnable = false,
                StencilEnable = true,
                StencilFunction = CompareFunction.NotEqual,
                ReferenceStencil = 0
            };

            colourWriteDisable = new BlendState()
            {
                ColorWriteChannels = ColorWriteChannels.None
            };
        }

        public void Prepare(Renderer renderer)
        {
            touchesNearPlane.Clear();
            touchesFarPlane.Clear();
            touchesBothPlanes.Clear();
            touchesNeitherPlane.Clear();

            var frustum = renderer.Data.Get<BoundingFrustum>("viewfrustum").Value;

            //float falloffFactor = renderer.Data.Get("lighting_attenuationscale", 100).Value;
            //geometryLightingMaterial.Parameters["LightFalloffFactor"].SetValue(falloffFactor);
            //quadLightingMaterial.Parameters["LightFalloffFactor"].SetValue(falloffFactor);

            foreach (var light in Behaviours)
            {

                var bounds = new BoundingSphere(light.Position, light.Range);
                if (!bounds.Intersects(frustum))
                    continue;

                var near = bounds.Intersects(frustum.Near) == PlaneIntersectionType.Intersecting;
                var far = bounds.Intersects(frustum.Far) == PlaneIntersectionType.Intersecting;

                if (near && far)
                    touchesBothPlanes.Add(light);
                else if (near)
                    touchesNearPlane.Add(light);
                else if (far)
                    touchesFarPlane.Add(light);
                else
                    touchesNeitherPlane.Add(light);
            }
        }


        public void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var part = geometry.Meshes[0].MeshParts[0];
            device.SetVertexBuffer(part.VertexBuffer);
            device.Indices = part.IndexBuffer;

            DrawGeometryLights(touchesFarPlane, metadata, device);

            device.DepthStencilState = depthGreater;
            device.RasterizerState = RasterizerState.CullClockwise;
            DrawGeometryLights(touchesNearPlane, metadata, device);
            DrawGeometryLights(touchesNeitherPlane, metadata, device);

            //foreach (var light in touchesNeitherPlane)
            //{
            //    device.DepthStencilState = stencilWritePass;
            //    device.RasterizerState = RasterizerState.CullNone;
            //    device.BlendState = colourWriteDisable;
            //    device.Clear(ClearOptions.Stencil, Color.Transparent, 0, 0);

            //    SetupLight(metadata, null, light);
            //    DrawGeomery(nothingMaterial, metadata, device);

            //    device.DepthStencilState = stencilCheckPass;
            //    device.RasterizerState = RasterizerState.CullCounterClockwise;
            //    device.BlendState = BlendState.Additive;
            //    DrawGeomery(geometryLightingMaterial, metadata, device);
            //}

            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullCounterClockwise;

            foreach (var light in touchesBothPlanes)
            {
                SetupLight(metadata, quadLightingMaterial, light);
                quad.Draw(quadLightingMaterial, metadata);
            }
        }

        private void DrawGeometryLights(List<PointLight> lights, RendererMetadata metadata, GraphicsDevice device)
        {
            foreach (var light in lights)
            {
                SetupLight(metadata, geometryLightingMaterial, light);
                DrawGeomery(geometryLightingMaterial, metadata, device);
            }
        }

        private void DrawGeomery(Material material, RendererMetadata metadata, GraphicsDevice device)
        {
            var part = geometry.Meshes[0].MeshParts[0];
            foreach (var pass in material.Begin(metadata))
            {
                pass.Apply();

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
            }
        }

        private void SetupLight(RendererMetadata metadata, Material material, PointLight light)
        {
            if (material != null)
            {
                Vector3 position = Vector3.Transform(light.Position, metadata.Get<Matrix>("view").Value);
                material.Parameters["LightPosition"].SetValue(position);
                material.Parameters["Colour"].SetValue(light.Colour);
                material.Parameters["Range"].SetValue(light.Range);
            }

            var world = Matrix.CreateScale(light.Range / geometry.Meshes[0].BoundingSphere.Radius)
                        * Matrix.CreateTranslation(light.Position);
            metadata.Set<Matrix>("world", world);
            Matrix.Multiply(ref world, ref metadata.Get<Matrix>("view").Value, out metadata.Get<Matrix>("worldview").Value);
            Matrix.Multiply(ref metadata.Get<Matrix>("worldview").Value, ref metadata.Get<Matrix>("projection").Value, out metadata.Get<Matrix>("worldviewprojection").Value);
        }
    }
}
