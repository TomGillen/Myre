using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities;
using Myre.Graphics.Deferred;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Myre.Graphics.Geometry;
using Myre.Graphics.ModelViewer.Xna;
using Myre.Graphics.Lighting;

namespace Myre.Graphics.ModelViewer.Model
{
    public class SceneAdapter
    {
        private Scene scene;
        private RenderPlan finalOutputPlan;
        private Entity modelEntity;
        private View cameraView;
        private int currentWidth;
        private int currentHeight;

        private DateTime lastFrame;
        private bool initialised;

        public SceneAdapter()
        {
            scene = new Scene();
        }

        public void Initialise()
        {
            if (initialised)
                return;

            BindKernel(XnaResources.GraphicsDeviceService);

            var renderer = scene.GetService<Renderer>();

            finalOutputPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<Ssao>()
                .Then<LightingComponent>()
                .Then<ToneMapComponent>()
                .Show("tonemapped");

            finalOutputPlan.Apply();

            SetupLights();
            CreateCamera();

            lastFrame = DateTime.Now;
            initialised = true;
        }

        private void SetupLights()
        {
            var ambientDesc = new EntityDescription();
            ambientDesc.AddBehaviour<AmbientLight>();

            var ambient = ambientDesc.Create();
            ambient.GetProperty<Vector3>("sky_colour").Value = new Vector3(1, 1, 1);
            ambient.GetProperty<Vector3>("ground_colour").Value = new Vector3(1, 1, 1);
            ambient.GetProperty<Vector3>("up").Value = new Vector3(0, 1, 0);
            scene.Add(ambient);
        }

        private void CreateCamera()
        {
            if (cameraView != null)
                return;

            var cameraDesc = new EntityDescription(NinjectKernel.Instance);
            cameraDesc.AddBehaviour<View>();

            var camera = cameraDesc.Create();
            camera.GetProperty<Viewport>("viewport").Value = new Viewport(0, 0, 100, 100);
            camera.GetProperty<Camera>("camera").Value = new Camera() { NearClip = 1, FarClip = 100, View = Matrix.Identity, Projection = Matrix.CreateOrthographic(100, 100, 1, 100) };
            scene.Add(camera);

            cameraView = camera.GetBehaviour<View>();
        }

        private void BindKernel(GraphicsDeviceService device)
        {
            NinjectKernel.Instance.Bind<GraphicsDevice>().ToConstant(device.GraphicsDevice);

            var services = new ServiceContainer();
            services.AddService<IGraphicsDeviceService>(device);

            NinjectKernel.Instance.Bind<IServiceProvider>().ToConstant(services);
        }

        public void SetResolution(int width, int height)
        {
            if (width <= 0)
                width = 50;
            if (height <= 0)
                height = 50;

            if (currentHeight == width && currentHeight == height)
                return;

            CreateCamera();
            cameraView.Viewport = new Viewport(0, 0, width, height);
            currentWidth = width;
            currentHeight = height;
        }

        public void Draw()
        {
            var now = DateTime.Now;
            var timeDelta = now - lastFrame;
            lastFrame = now;
            scene.Update((float)timeDelta.Seconds);

            scene.Draw();
        }

        public void DisplayModel(ModelData model)
        {
            if (modelEntity == null)
            {
                var desc = new EntityDescription(NinjectKernel.Instance);
                desc.AddBehaviour<ModelInstance>();

                modelEntity = desc.Create();
                modelEntity.GetProperty<Matrix>("transform").Value = Matrix.Identity;
            }
            else
                scene.Remove(modelEntity);

            modelEntity.GetProperty<ModelData>("model").Value = model;
            scene.Add(modelEntity);

            var camera = cameraView.Camera;
            var radius = model.Meshes[0].BoundingSphere.Radius * 2;
            camera.View = Matrix.CreateLookAt(new Vector3(0, radius, -radius), Vector3.Zero, Vector3.Up);
            camera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(75), cameraView.Viewport.AspectRatio, 1, radius * 2);
            camera.NearClip = 1;
            camera.FarClip = radius * 2;
        }
    }
}
