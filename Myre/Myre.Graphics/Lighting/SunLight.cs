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
using Myre.Debugging;

namespace Myre.Graphics.Lighting
{
    [DefaultManager(typeof(Manager))]
    public class SunLight
        : Behaviour
    {
        private Property<Vector3> colour;
        private Property<Vector3> direction;
        private Property<int> shadowResolution;
        private RenderTarget2D shadowMap;
        //private Matrix[] shadowViewMatrices = new Matrix[4];
        //private Matrix[] shadowProjectionMatrices = new Matrix[4];
        //private float[] farClip = new float[4];
        private Matrix view;
        private Matrix projection;
        private float farClip;
        private Vector4 nearPlane;

        public Vector3 Colour
        {
            get { return colour.Value; }
            set { colour.Value = value; }
        }

        public Vector3 Direction
        {
            get { return direction.Value; }
            set { direction.Value = Vector3.Normalize(value); }
        }

        public int ShadowResolution
        {
            get { return shadowResolution.Value; }
            set { shadowResolution.Value = value; }
        }

        public override void Initialise(Entity.InitialisationContext context)
        {
            this.colour = context.GetOrCreateProperty<Vector3>("colour");
            this.direction = context.GetOrCreateProperty<Vector3>("direction");
            this.shadowResolution = context.GetOrCreateProperty<int>("shadow_resolution");
            base.Initialise(context);
        }

        /*
        [Inject, Name("colour")]
        public Property<Vector3> Colour { get; set; }

        [Inject, Name("direction")]
        public Property<Vector3> Direction { get; set; }

        [Inject, Name("shadowresolution")]
        public Property<int> ShadowResolution { get; set; }
        */

        public class Manager
            : BehaviourManager<SunLight>, ILightProvider
        {

            public static RenderTarget2D shadowmap;

            private Material material;
            private Quad quad;
            private List<ICullable> visibilityResults;

            private Vector3[] frustumCornersWS;
            private Vector3[] frustumCornersVS;
            //private Vector3[] splitFrustumCornersVS;
            //private Vector3[] splitFrustumCornersWS;
            //private BoundingSphere[] splitBounds;
            //private Vector3[] splitCorners;
            //private float[] splitDepths;
            //private BoundingVolume boundingVolume;
            //private List<ICullable> buffer;
            private View shadowView;

            public bool ModifiesStencil
            {
                get { return false; }
            }

            public Manager(
                IKernel kernel,
                ContentManager content,
                GraphicsDevice device,
                [SceneService] Renderer renderer)
            {
                var effect = content.Load<Effect>("DirectionalLight");
                material = new Material(effect, null);

                quad = new Quad(device);
                visibilityResults = new List<ICullable>();
                
                frustumCornersWS = new Vector3[8];
                frustumCornersVS = new Vector3[8];
                //splitFrustumCornersVS = new Vector3[20];
                //splitFrustumCornersWS = new Vector3[20];
                //splitBounds = new BoundingSphere[4];
                //splitCorners = new Vector3[8];
                //splitDepths = new float[5];
                //boundingVolume = new BoundingVolume();
                //buffer = new List<ICullable>();

                var shadowCameraEntity = kernel.Get<EntityDescription>();
                shadowCameraEntity.AddBehaviour<View>();
                shadowView = shadowCameraEntity.Create().GetBehaviour<View>();
                shadowView.Camera = new Camera();

                DebugShapeRenderer.Initialize(device);
            }

            //public void Draw(Renderer renderer)
            //{
            //    var metadata = renderer.Data;

            //    foreach (var light in Behaviours)
            //    {
            //        Vector3 direction = light.Direction;
            //        Vector3.TransformNormal(ref direction, ref metadata.Get<Matrix>("view").Value, out direction);
            //        material.Parameters["Direction"].SetValue(direction);
            //        material.Parameters["Colour"].SetValue(light.Colour);

            //        quad.Draw(material, metadata);
            //    }
            //}

            //public bool PrepareDraw(Renderer renderer)
            //{
            //    CalculateSplits(renderer);

            //    for (int i = 0; i < Behaviours.Count; i++)
            //    {
            //        var light = Behaviours[i];

            //        if (light.shadowMap != null)
            //        {
            //            RenderTargetManager.RecycleTarget(light.shadowMap);
            //            light.shadowMap = null;
            //        }

            //        if (light.ShadowResolution != 0)
            //        {
            //            CalculateShadowMatrices(renderer, light);
            //            DrawShadowMap(renderer, light);
            //        }
            //    }


            //    return false;
            //}

            //private void CalculateSplits(Renderer renderer)
            //{
            //    // find frustum corners
            //    var viewFrustum = renderer.Data.Get<BoundingFrustum>("viewfrustum").Value;
            //    viewFrustum.GetCorners(frustumCornersWS);
            //    Vector3.Transform(frustumCornersWS, ref renderer.Data.Get<Matrix>("view").Value, frustumCornersVS);

            //    // calculate split distances
            //    // [from sample by mjp (mynameismjp.wordpress.com)]
            //    float N = 4;
            //    float near = renderer.Data.Get<float>("nearclip").Value;
            //    float far = renderer.Data.Get<float>("farclip").Value;
            //    splitDepths[0] = near;
            //    splitDepths[4] = far;
            //    const float splitConstant = 0.95f;
            //    for (int i = 1; i < splitDepths.Length - 1; i++)
            //        splitDepths[i] = splitConstant * near * (float)Math.Pow(far / near, i / N) + (1.0f - splitConstant) * ((near + (i / N)) * (far - near));

            //    // calculate the split corners
            //    for (int i = 0; i < splitDepths.Length; i++)
            //    {
            //        for (int j = 0; j < 4; j++)
            //            splitFrustumCornersVS[i * 4 + j] = frustumCornersVS[j + 4] * (splitDepths[i] / far);
            //    }

            //    Vector3.Transform(splitFrustumCornersVS, ref renderer.Data.Get<Matrix>("inverseview").Value, splitFrustumCornersWS);

            //    // calculate split bounding spheres
            //    for (int i = 0; i < splitBounds.Length; i++)
            //    {
            //        for (int j = 0; j < 4; j++)
            //        {
            //            splitCorners[j] = splitFrustumCornersWS[i * 4 + j];
            //            splitCorners[j + 4] = splitFrustumCornersWS[i * 4 + j + 4];
            //        }

            //        splitBounds[i] = BoundingSphere.CreateFromPoints(splitCorners);
            //    }
            //}

            //private void CalculateShadowMatrices(Renderer renderer, SunLight light)
            //{
            //    // get far frustum split corners in view space
            //    // calculate split distances
            //    // for each split:
            //    //   calculate frustum corners
            //    //   place bounding sphere
            //    //   quantise sphere centre
            //    //   calculate bounding volume around sphere, square prism of infinte length along light direction
            //    //   query items in volume, find furthest extent towards the light
            //    //   calculate othogonal projection matrix

            //    // calculate axis vectors of the light
            //    var up = (light.Direction == Vector3.UnitY || light.Direction == -Vector3.UnitY) ? Vector3.UnitX : Vector3.UnitY;
            //    var lightRotation = Matrix.CreateLookAt(Vector3.Zero, -light.Direction, up);
            //    var zAxis = lightRotation.Forward;
            //    var xAxis = lightRotation.Left;
            //    var yAxis = lightRotation.Up;

            //    var shadowMapSize = light.ShadowResolution;

            //    // for each split
            //    for (int i = 0; i < splitBounds.Length; i++)
            //    {
            //        Vector3 centre = splitBounds[i].Center;
            //        float radius = splitBounds[i].Radius;

            //        // quantise split centre to shadow map texels
            //        /*
            //        float x = (float)Math.Ceiling(Vector3.Dot(centre, xAxis) * shadowMapSize / radius) * radius / shadowMapSize;
            //        float y = (float)Math.Ceiling(Vector3.Dot(centre, yAxis) * shadowMapSize / radius) * radius / shadowMapSize;
            //        centre = xAxis * x + yAxis * y + zAxis * Vector3.Dot(centre, light.Direction);

            //        boundingVolume.Clear();
            //        boundingVolume.Add(new Plane(-zAxis, -radius));
            //        boundingVolume.Add(new Plane(xAxis, -radius));
            //        boundingVolume.Add(new Plane(-xAxis, radius));
            //        boundingVolume.Add(new Plane(yAxis, -radius));
            //        boundingVolume.Add(new Plane(-yAxis, radius));

            //        float extent = FindFurthestExtent(renderer, boundingVolume, zAxis) - 1;

            //        var centreProjected = Vector3.Dot(centre, zAxis);
            //        var distance = extent - centreProjected;
            //        var lightPosition = centre + zAxis * distance;

            //        light.farClip[i] = radius - distance;
            //        light.shadowViewMatrices[i] = Matrix.Invert(Matrix.CreateTranslation(lightPosition) * lightRotation);
            //        light.shadowPojectionMatrices[i] = Matrix.CreateOrthographic(radius * 2, radius * 2, 1, light.farClip[i]);
            //         * */

            //        var lightPosition = centre + -light.Direction * radius;
            //        light.farClip[i] = radius * 2;
            //        light.shadowViewMatrices[i] = Matrix.CreateLookAt(lightPosition, centre, up);
            //        light.shadowProjectionMatrices[i] = Matrix.CreateOrthographic(radius * 2, radius * 2, 1, radius * 2);
            //    }
            //}

            //private void DrawShadowMap(Renderer renderer, SunLight light)
            //{
            //    var target = RenderTargetManager.GetTarget(renderer.Device, light.ShadowResolution /* * 2*/, light.ShadowResolution /* * 2*/, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            //    renderer.Device.SetRenderTarget(target);
            //    renderer.Device.Clear(Color.Black);

            //    var resolution = renderer.Data.Get<Vector2>("resolution");
            //    var previousResolution = resolution.Value;
            //    resolution.Value = new Vector2(light.ShadowResolution);

            //    renderer.Device.DepthStencilState = DepthStencilState.Default;
            //    renderer.Device.BlendState = BlendState.Opaque;
            //    renderer.Device.RasterizerState = RasterizerState.CullCounterClockwise;

            //    var view = renderer.Data.Get<View>("activeview");
            //    var previousView = view.Value;
            //    view.Value = shadowView;

            //    //var previousViewport = renderer.Device.Viewport;

            //    for (int i = 0; i < 1/*light.shadowViewMatrices.Length*/; i++)
            //    {
            //        shadowView.Camera.View = light.shadowViewMatrices[i];
            //        shadowView.Camera.Projection = light.shadowProjectionMatrices[i];
            //        shadowView.Camera.NearClip = 1;
            //        shadowView.Camera.FarClip = light.farClip[i];

            //        shadowView.Viewport = new Viewport()
            //        {
            //            X = 0,// (i % 2) * light.ShadowResolution;
            //            Y = 0,// (i > 1) ? light.ShadowResolution : 0;
            //            Width = light.ShadowResolution,
            //            Height = light.ShadowResolution
            //        };
                    
            //        shadowView.SetMetadata(renderer.Data);
            //        //renderer.Device.Viewport = shadowView.Viewport;

            //        foreach (var item in renderer.Scene.FindManagers<IGeometryProvider>())
            //            item.Draw("shadows", renderer.Data);
            //    }

            //    light.shadowMap = target;
            //    resolution.Value = previousResolution;
            //    previousView.SetMetadata(renderer.Data);
            //    view.Value = previousView;
            //    //renderer.Device.Viewport = previousViewport;
            //}

            //private float FindFurthestExtent(Renderer renderer, BoundingVolume boundingVolume, Vector3 axis)
            //{
            //    foreach (var item in renderer.Scene.FindManagers<IGeometryProvider>())
            //        item.Query(buffer, boundingVolume);

            //    float extent = 0;
            //    foreach (var item in buffer)
            //    {
            //        float distance = Vector3.Dot(item.Bounds.Center, axis) - item.Bounds.Radius;
            //        if (distance < extent)
            //            extent = distance;
            //    }

            //    buffer.Clear();
            //    return extent;
            //}

            public void DrawDebug(Renderer renderer)
            {
                foreach (var light in Behaviours)
                {
                    if (light.ShadowResolution > 0)
                    {
                        var bounds = new BoundingFrustum(light.view * light.projection);
                        DebugShapeRenderer.AddBoundingFrustum(bounds, new Color(light.Colour));
                    }
                }

                var view = renderer.Data.Get<Matrix>("view").Value;
                var projection = renderer.Data.Get<Matrix>("projection").Value;
                DebugShapeRenderer.Draw(new GameTime(), view, projection);
            }

            public void Draw(Renderer renderer)
            {
                foreach (var light in Behaviours)
                {
                    SetupLight(renderer.Data, light);
                    quad.Draw(material, renderer.Data);
                }
            }

            private void SetupLight(RendererMetadata metadata, SunLight light)
            {
                if (material != null)
                {
                    Matrix view = metadata.Get<Matrix>("view").Value;
                    Vector3 direction = light.Direction;
                    Vector3.TransformNormal(ref direction, ref view, out direction);

                    var shadowsEnabled = light.ShadowResolution > 0;

                    material.Parameters["Direction"].SetValue(direction);
                    material.Parameters["Colour"].SetValue(light.Colour);
                    material.Parameters["EnableShadows"].SetValue(shadowsEnabled);

                    if (shadowsEnabled)
                    {
                        material.Parameters["ShadowProjection"].SetValue(metadata.Get<Matrix>("inverseview").Value * light.view * light.projection);
                        material.Parameters["ShadowMapSize"].SetValue(new Vector2(light.ShadowResolution, light.ShadowResolution));
                        material.Parameters["ShadowMap"].SetValue(light.shadowMap);
                        material.Parameters["LightFarClip"].SetValue(light.farClip);
                        material.Parameters["LightNearPlane"].SetValue(light.nearPlane);
                    }
                }
            }

            public bool PrepareDraw(Renderer renderer)
            {
                renderer.Data.Get<BoundingFrustum>("viewfrustum").Value.GetCorners(frustumCornersWS);

                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var light = Behaviours[i];
                    light.Direction = Vector3.Normalize(light.Direction);

                    if (light.shadowMap != null)
                    {
                        RenderTargetManager.RecycleTarget(light.shadowMap);
                        light.shadowMap = null;
                    }

                    if (light.ShadowResolution != 0)
                    {
                        CalculateShadowMatrices(renderer, light);
                        DrawShadowMap(renderer, light);
                    }
                }


                return false;
            }

            private void CalculateShadowMatrices(Renderer renderer, SunLight light)
            {
                var min = float.PositiveInfinity;
                var max = float.NegativeInfinity;
                for (int i = 0; i < frustumCornersWS.Length; i++)
                {
                    var projection = Vector3.Dot(frustumCornersWS[i], light.Direction);
                    min = Math.Min(min, projection);
                    max = Math.Max(max, projection);
                }

                min = -500;
                max = 500;

                var depthOffset = -min;
                var lightPosition = -light.Direction * depthOffset;
                var lightIsVertical = light.Direction == Vector3.Up || light.Direction == Vector3.Down;
                var viewMatrix = Matrix.CreateLookAt(lightPosition, Vector3.Zero, lightIsVertical ? Vector3.Forward : Vector3.Up);

                Vector3.Transform(frustumCornersWS, ref viewMatrix, frustumCornersVS);
                
                var bounds = BoundingSphere.CreateFromPoints(frustumCornersVS);

                var farClip = max - min;
                var projectionMatrix = Matrix.CreateOrthographicOffCenter(-bounds.Radius, bounds.Radius, -bounds.Radius, bounds.Radius, 0, farClip);

                light.view = viewMatrix;
                light.projection = projectionMatrix;
                light.farClip = farClip;        

                var nearPlane = new Plane(light.Direction, depthOffset);
                nearPlane.Normalize();
                Plane transformedNearPlane;
                Plane.Transform(ref nearPlane, ref renderer.Data.Get<Matrix>("view").Value, out transformedNearPlane);
                light.nearPlane = new Vector4(transformedNearPlane.Normal, transformedNearPlane.D);
            }

            private void DrawShadowMap(Renderer renderer, SunLight light)
            {
                var target = RenderTargetManager.GetTarget(renderer.Device, light.ShadowResolution, light.ShadowResolution, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, name:"sun light shadow map");
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

                shadowView.Camera.View = light.view;
                shadowView.Camera.Projection = light.projection;
                shadowView.Camera.NearClip = 1;
                shadowView.Camera.FarClip = light.farClip;
                shadowView.Viewport = new Viewport(0, 0, light.ShadowResolution, light.ShadowResolution);
                shadowView.SetMetadata(renderer.Data);

                foreach (var item in renderer.Scene.FindManagers<IGeometryProvider>())
                    item.Draw("shadows_viewz", renderer.Data);

                light.shadowMap = target;
                resolution.Value = previousResolution;
                previousView.SetMetadata(renderer.Data);
                view.Value = previousView;

                shadowmap = target;
            }
        }
    }
}
