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
using Myre.Graphics.Geometry;
using System.Diagnostics;
using Myre.Debugging.Statistics;

namespace Myre.Graphics.Lighting
{
    [DefaultManager(typeof(Manager))]
    public class SpotLight
        : Behaviour
    {
        /*
        [Inject, Name("colour")]
        public Property<Vector3> Colour { get; set; }

        [Inject, Name("position")]
        public Property<Vector3> Position { get; set; }

        [Inject, Name("direction")]
        public Property<Vector3> Direction { get; set; }

        [Inject, Name("angle")]
        public Property<float> Angle { get; set; }

        [Inject, Name("mask")]
        public Property<Texture2D> Mask { get; set; }

        [Inject, Name("shadowresolution")]
        public Property<int> ShadowResolution { get; set; }
        */

        private Property<Vector3> colour;
        private Property<Vector3> position;
        private Property<Vector3> direction;
        private Property<float> angle;
        private Property<Texture2D> mask;
        private Property<int> shadowResolution;
        private float range;
        private RenderTarget2D shadowMap;
        private Matrix view;
        private Matrix projection;

        public Vector3 Colour
        {
            get { return colour.Value; }
            set { colour.Value = value; }
        }

        public Vector3 Position
        {
            get { return position.Value; }
            set { position.Value = value; }
        }

        public Vector3 Direction
        {
            get { return direction.Value; }
            set { direction.Value = Vector3.Normalize(value); }
        }

        public float Angle
        {
            get { return angle.Value; }
            set { angle.Value = value; }
        }

        public Texture2D Mask
        {
            get { return mask.Value; }
            set { mask.Value = value; }
        }

        public int ShadowResolution
        {
            get { return shadowResolution.Value; }
            set { shadowResolution.Value = value; }
        }

        public override void Initialise(Entity.InitialisationContext context)
        {
            this.colour = context.GetOrCreateProperty<Vector3>("colour");
            this.position = context.GetOrCreateProperty<Vector3>("position");
            this.direction = context.GetOrCreateProperty<Vector3>("direction");
            this.angle = context.GetOrCreateProperty<float>("angle");
            this.mask = context.GetOrCreateProperty<Texture2D>("mask");
            this.shadowResolution = context.GetOrCreateProperty<int>("shadow_resolution");
            base.Initialise(context);
        }


        public class Manager
            : BehaviourManager<SpotLight>, ILightProvider
        {
            private Material geometryLightingMaterial;
            private Material quadLightingMaterial;
            private Material nothingMaterial;
            private Quad quad;
            private Model geometry;
            private View shadowView;

            private BasicEffect basicEffect;
            private VertexPositionColor[] debugVertices;
            private int[] debugIndices;

            private List<SpotLight> touchesNearPlane;
            private List<SpotLight> touchesFarPlane;
            private List<SpotLight> touchesBothPlanes;
            private List<SpotLight> touchesNeitherPlane;

            private DepthStencilState depthGreater;
            private BlendState colourWriteDisable;
            private DepthStencilState stencilWritePass;
            private DepthStencilState stencilCheckPass;

            public bool ModifiesStencil
            {
                get { return true; }
            }

            public Manager(
                IKernel kernel,
                ContentManager content,
                GraphicsDevice device,
                [SceneService] Renderer renderer)
            {            
                var effect = content.Load<Effect>("SpotLight");
                geometryLightingMaterial = new Material(effect.Clone(), "Geometry");
                quadLightingMaterial = new Material(effect.Clone(), "Quad");

                effect = content.Load<Effect>("Nothing");
                nothingMaterial = new Material(effect, null);

                basicEffect = new BasicEffect(device);
                debugVertices = new VertexPositionColor[10];
                debugIndices = new int[(debugVertices.Length - 1) * 2 * 2];
                for (int i = 1; i < debugVertices.Length; i++)
                {
                    debugVertices[i] = new VertexPositionColor(
                        new Vector3(
                            (float)Math.Sin(i * (MathHelper.TwoPi / (debugVertices.Length - 1))),
                            (float)Math.Cos(i * (MathHelper.TwoPi / (debugVertices.Length - 1))),
                            -1),
                        Color.White);

                    var index = (i - 1) * 4;
                    debugIndices[index] = 0;
                    debugIndices[index + 1] = i;
                    debugIndices[index + 2] = i;
                    debugIndices[index + 3] = (i % (debugVertices.Length - 1)) + 1; //i < debugVertices.Length - 1 ? i + 1 : 1;
                }                

                geometry = content.Load<Model>("sphere");

                quad = new Quad(device);

                var shadowCameraEntity = kernel.Get<EntityDescription>();
                shadowCameraEntity.AddBehaviour<View>();
                shadowView = shadowCameraEntity.Create().GetBehaviour<View>();
                shadowView.Camera = new Camera();

                touchesNearPlane = new List<SpotLight>();
                touchesFarPlane = new List<SpotLight>();
                touchesBothPlanes = new List<SpotLight>();
                touchesNeitherPlane = new List<SpotLight>();

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

            public bool PrepareDraw(Renderer renderer)
            {
                touchesNearPlane.Clear();
                touchesFarPlane.Clear();
                touchesBothPlanes.Clear();
                touchesNeitherPlane.Clear();

                var frustum = renderer.Data.Get<BoundingFrustum>("viewfrustum").Value;

                float falloffFactor = renderer.Data.Get("lighting_attenuationscale", 100).Value;
                geometryLightingMaterial.Parameters["LightFalloffFactor"].SetValue(falloffFactor);
                quadLightingMaterial.Parameters["LightFalloffFactor"].SetValue(falloffFactor);

                float threshold = renderer.Data.Get("lighting_threshold", 1f / 100f).Value;
                float adaptedLuminance = renderer.Data.Get<float>("adaptedluminance", 1).Value;

                threshold = adaptedLuminance * threshold;

                foreach (var light in Behaviours)
                {
                    var luminance = Math.Max(light.Colour.X, Math.Max(light.Colour.Y, light.Colour.Z));
                    light.range = (float)Math.Sqrt(luminance * falloffFactor / threshold);

                    var bounds = new BoundingSphere(light.Position, light.range);
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

                    if (light.shadowMap != null)
                    {
                        RenderTargetManager.RecycleTarget(light.shadowMap);
                        light.shadowMap = null;
                    }
                    
                    if (light.ShadowResolution > 0)
                        DrawShadowMap(renderer, light);
                }

                return true;
            }

            private void DrawShadowMap(Renderer renderer, SpotLight light)
            {
                var target = RenderTargetManager.GetTarget(renderer.Device, light.ShadowResolution, light.ShadowResolution, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
                renderer.Device.SetRenderTarget(target);
                renderer.Device.Clear(Color.Black);

                var resolution = renderer.Data.Get<Vector2>("resolution");
                var previousResolution = resolution.Value;
                resolution.Value = new Vector2(light.ShadowResolution);

                renderer.Device.DepthStencilState = DepthStencilState.Default;
                renderer.Device.BlendState = BlendState.Opaque;
                renderer.Device.RasterizerState = RasterizerState.CullCounterClockwise;

                var view = renderer.Data.Get<View>("activeview");
                var previousView = view.Value;
                view.Value = shadowView;

                light.view = Matrix.CreateLookAt(
                    light.Position,
                    light.Position + light.Direction,
                    light.Direction == Vector3.Up || light.Direction == Vector3.Down ? Vector3.Right : Vector3.Up);
                light.projection = Matrix.CreatePerspectiveFieldOfView(light.Angle, 1, 1, light.range);

                shadowView.Camera.View = light.view;
                shadowView.Camera.Projection = light.projection;
                shadowView.Camera.NearClip = 1;
                shadowView.Camera.FarClip = light.range;
                shadowView.Viewport = new Viewport(0, 0, light.ShadowResolution, light.ShadowResolution);
                shadowView.SetMetadata(renderer.Data);

                foreach (var item in renderer.Scene.FindManagers<IGeometryProvider>())
                    item.Draw("shadows", renderer.Data);

                light.shadowMap = target;
                resolution.Value = previousResolution;
                previousView.SetMetadata(renderer.Data);
                view.Value = previousView;
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

            public void DrawDebug(Renderer renderer)
            {
                basicEffect.View = renderer.Data.Get<Matrix>("view").Value;
                basicEffect.Projection = renderer.Data.Get<Matrix>("projection").Value;

                foreach (var light in Behaviours)
                {
                    var transform = Matrix.CreateLookAt(
                        light.Position,
                        light.Position + light.Direction,
                        light.Direction == Vector3.Up || light.Direction == Vector3.Down ? Vector3.Right : Vector3.Up);
                    transform = Matrix.Invert(transform);

                    basicEffect.DiffuseColor = light.Colour;
                    basicEffect.World = transform;

                    basicEffect.CurrentTechnique.Passes[0].Apply();
                    renderer.Device.DrawUserIndexedPrimitives(PrimitiveType.LineList, debugVertices, 0, debugVertices.Length, debugIndices, 0, debugIndices.Length / 2);
                }
            }

            private void DrawGeometryLights(List<SpotLight> lights, RendererMetadata metadata, GraphicsDevice device)
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

            private void SetupLight(RendererMetadata metadata, Material material, SpotLight light)
            {
                if (material != null)
                {
                    Matrix view = metadata.Get<Matrix>("view").Value;
                    Vector3 position = light.Position;
                    Vector3 direction = light.Direction;
                    Vector3.Transform(ref position, ref view, out position);
                    Vector3.TransformNormal(ref direction, ref view, out direction);
                    float angle = (float)Math.Cos(light.Angle / 2);

                    if (light.Mask != null || light.ShadowResolution > 0)
                    {
                        var inverseView = metadata.Get<Matrix>("inverseview").Value;
                        var cameraToLightView = inverseView * light.view;
                        var cameraToLightProjection = cameraToLightView * light.projection;
                        material.Parameters["CameraViewToLightProjection"].SetValue(cameraToLightProjection);
                        material.Parameters["CameraViewToLightView"].SetValue(cameraToLightView);
                        material.Parameters["LightFarClip"].SetValue(light.range);
                    }

                    material.Parameters["LightPosition"].SetValue(position);
                    material.Parameters["LightDirection"].SetValue(-direction);
                    material.Parameters["Angle"].SetValue(angle);
                    material.Parameters["Range"].SetValue(light.range);
                    material.Parameters["Colour"].SetValue(light.Colour);
                    material.Parameters["EnableProjectiveTexturing"].SetValue(light.Mask != null);
                    material.Parameters["Mask"].SetValue(light.Mask);
                    material.Parameters["EnableShadows"].SetValue(light.ShadowResolution > 0);
                    material.Parameters["ShadowMapSize"].SetValue(new Vector2(light.ShadowResolution, light.ShadowResolution));
                    material.Parameters["ShadowMap"].SetValue(light.shadowMap);
                }

                var world = Matrix.CreateScale(light.range / geometry.Meshes[0].BoundingSphere.Radius)
                            * Matrix.CreateTranslation(light.Position);
                metadata.Set<Matrix>("world", world);
                Matrix.Multiply(ref world, ref metadata.Get<Matrix>("view").Value, out metadata.Get<Matrix>("worldview").Value);
                Matrix.Multiply(ref metadata.Get<Matrix>("worldview").Value, ref metadata.Get<Matrix>("projection").Value, out metadata.Get<Matrix>("worldviewprojection").Value);
            }
        }
    }
}
