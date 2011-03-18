using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities;
using Ninject;
using Microsoft.Xna.Framework;
using Myre.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Geometry;
using Myre.Graphics.Lighting;
using Myre.UI;
using Myre.Debugging;
using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.Input;
using Myre.Collections;
using Myre.Graphics.Particles;

namespace GraphicsTests
{
    class TestScene
    {
        Game game;
        Scene scene;
        Camera camera;
        List<PointLight> lights;
        SpotLight spotLight;
        Entity sun;
        Box<Vector2> resolution;
        SpriteBatch sb;
        Effect basic;

        Vector3 cameraPosition;
        Vector3 cameraRotation;

        public Scene Scene
        {
            get { return scene; }
        }

        public Camera Camera
        {
            get { return camera; }
        }

        public TestScene(IKernel kernel, Game game, ContentManager content, GraphicsDevice device)
        {
            scene = new Scene(kernel);
            this.game = game;

            sb = new SpriteBatch(device);
            basic = content.Load<Effect>("Basic");

            cameraPosition = new Vector3(100, 50, 0);
            
            camera = new Camera();
            camera.NearClip = 1;
            camera.FarClip = 700;
            camera.View = Matrix.CreateLookAt(cameraPosition, new Vector3(0, 50, 0), Vector3.Up);
            camera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, camera.NearClip, camera.FarClip);

            var cameraEntity = kernel.Get<EntityDescription>();
            cameraEntity.AddProperty<Camera>("camera", camera);
            cameraEntity.AddProperty<Viewport>("viewport", new Viewport() { Width = device.PresentationParameters.BackBufferWidth, Height = device.PresentationParameters.BackBufferHeight });
            cameraEntity.AddBehaviour<View>();
            scene.Add(cameraEntity.Create());

            //var skyboxEntity = kernel.Get<EntityDescription>();
            //skyboxEntity.AddProperty<TextureCube>("texture", content.Load<TextureCube>("GraceCathedral"));
            //skyboxEntity.AddProperty<float>("brightness", 1);
            //skyboxEntity.AddProperty<bool>("gamma_correct", false);
            //skyboxEntity.AddBehaviour<Skybox>();
            //scene.Add(skyboxEntity.Create());

            //var sunEntity = kernel.Get<EntityDescription>();
            //sunEntity.AddProperty<Vector3>("direction", Vector3.Normalize(new Vector3(-.2f, -1f, .3f)));
            //sunEntity.AddProperty<Vector3>("colour", new Vector3(5f));
            //sunEntity.AddProperty<int>("shadow_resolution", 4096);
            //sunEntity.AddBehaviour<SunLight>();
            //sun = sunEntity.Create();
            //scene.Add(sun);

            //var sun2 = sunEntity.Create();
            //sun2.GetProperty<Vector3>("direction").Value = Vector3.Normalize(new Vector3(1, -1, 0));
            //sun2.GetProperty<Vector3>("colour").Value = new Vector3(1, 0, 0);
            //scene.Add(sun2);

            var pointLight = kernel.Get<EntityDescription>();
            pointLight.AddProperty<Vector3>("position", new Vector3(0, 10, 0));
            pointLight.AddProperty<Vector3>("colour", new Vector3(0, 5, 0));
            pointLight.AddBehaviour<PointLight>();
            //scene.Add(pointLight.Create());

            lights = new List<PointLight>();
            var rng = new Random();
            for (int i = 0; i < 0; i++)
            {
                var entity = pointLight.Create();
                scene.Add(entity);

                var light = entity.GetBehaviour<PointLight>();
                light.Colour = Vector3.Normalize(new Vector3(0.1f + (float)rng.NextDouble(), 0.1f + (float)rng.NextDouble(), 0.1f + (float)rng.NextDouble())) * 10;
                lights.Add(light);
            }

            var spotLight = kernel.Get<EntityDescription>();
            spotLight.AddProperty<Vector3>("position", new Vector3(-180, 250, 0));
            spotLight.AddProperty<Vector3>("colour", new Vector3(700));
            spotLight.AddProperty<Vector3>("direction", new Vector3(0, -1, 0));
            spotLight.AddProperty<float>("angle", MathHelper.PiOver2);
            spotLight.AddProperty<Texture2D>("mask", null);//content.Load<Texture2D>("Chrysanthemum"));
            spotLight.AddProperty<int>("shadow_resolution", 1024);
            spotLight.AddBehaviour<SpotLight>();
            var spotLightEntity = spotLight.Create();
            this.spotLight = spotLightEntity.GetBehaviour<SpotLight>();
            scene.Add(spotLightEntity);

            var ambientLight = kernel.Get<EntityDescription>();
            ambientLight.AddProperty<Vector3>("sky_colour", new Vector3(0.04f));
            ambientLight.AddProperty<Vector3>("ground_colour", new Vector3(0.02f, 0.02f, 0.03f));
            ambientLight.AddProperty<Vector3>("up", Vector3.Up);
            ambientLight.AddBehaviour<AmbientLight>();
            scene.Add(ambientLight.Create());

            var floor = content.Load<ModelData>(@"Models\Ground");
            var floorEntity = kernel.Get<EntityDescription>();
            floorEntity.AddProperty<ModelData>("model", floor);
            floorEntity.AddProperty<Matrix>("transform", Matrix.CreateScale(2));
            floorEntity.AddProperty<bool>("isstatic", true);
            floorEntity.AddBehaviour<ModelInstance>();
            scene.Add(floorEntity.Create());

            var lizard = content.Load<ModelData>(@"Models\lizard");
            var lizardEntity = kernel.Get<EntityDescription>();
            lizardEntity.AddProperty<ModelData>("model", lizard);
            lizardEntity.AddProperty<Matrix>("transform", Matrix.CreateScale(50 / 700f) * Matrix.CreateTranslation(150, 0, 0));
            lizardEntity.AddProperty<bool>("is_static", true);
            lizardEntity.AddBehaviour<ModelInstance>();
            scene.Add(lizardEntity.Create());

            //var ship1 = content.Load<ModelData>(@"Models\Ship1");
            //var ship1Entity = kernel.Get<EntityDescription>();
            //ship1Entity.AddProperty<ModelData>("model", ship1);
            //ship1Entity.AddProperty<Matrix>("transform", Matrix.CreateTranslation(30, 0, 0));
            //ship1Entity.AddProperty<bool>("is_static", true);
            //ship1Entity.AddBehaviour<ModelInstance>();
            //scene.Add(ship1Entity.Create());

            var hebe = content.Load<ModelData>(@"Models\Hebe2");
            var hebeEntity = kernel.Get<EntityDescription>();
            hebeEntity.AddProperty<ModelData>("model", hebe);
            hebeEntity.AddProperty<Matrix>("transform", Matrix.CreateScale(25 / hebe.Meshes[0].BoundingSphere.Radius)
                                                        * Matrix.CreateRotationY(MathHelper.PiOver2)
                                                        * Matrix.CreateTranslation(-150, 20, 0));
            hebeEntity.AddProperty<bool>("is_static", true);
            hebeEntity.AddBehaviour<ModelInstance>();
            scene.Add(hebeEntity.Create());

            var sponza = content.Load<ModelData>(@"Sponza");
            var sponzaEntity = kernel.Get<EntityDescription>();
            sponzaEntity.AddProperty<ModelData>("model", sponza);
            sponzaEntity.AddProperty<Matrix>("transform", Matrix.Identity * Matrix.CreateScale(1));
            sponzaEntity.AddProperty<bool>("is_static", true);
            sponzaEntity.AddBehaviour<ModelInstance>();
            scene.Add(sponzaEntity.Create());

            var renderer = scene.GetService<Renderer>();
            resolution = renderer.Data.Get<Vector2>("resolution");

            var console = kernel.Get<CommandConsole>();
            renderer.Settings.BindCommandEngine(console.Engine);

            var fire1 = Fire.Create(kernel, content, new Vector3(123.5f, 32f, -55f));
            var fire2 = Fire.Create(kernel, content, new Vector3(123.5f, 32f, 35f));

            scene.Add(fire1);
            scene.Add(fire2);
        }

        public void Update(GameTime gameTime)
        {
            var totalTime = (float)gameTime.TotalGameTime.TotalSeconds / 2;
            var time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();

            if (mouse.IsButtonDown(MouseButtons.Right))
            {
                var mousePosition = new Vector2(mouse.X, mouse.Y);
                var mouseDelta = mousePosition - resolution.Value / 2;

                cameraRotation.Y -= mouseDelta.X * time * 0.1f;
                cameraRotation.X -= mouseDelta.Y * time * 0.1f;

                var rotation = Matrix.CreateFromYawPitchRoll(cameraRotation.Y, cameraRotation.X, cameraRotation.Z);
                var forward = Vector3.TransformNormal(Vector3.Forward, rotation);
                var right = Vector3.TransformNormal(Vector3.Right, rotation);

                forward.Normalize();
                right.Normalize();

                if (keyboard.IsKeyDown(Keys.W))
                    cameraPosition += forward * time * 50;
                if (keyboard.IsKeyDown(Keys.S))
                    cameraPosition -= forward * time * 50f;
                if (keyboard.IsKeyDown(Keys.A))
                    cameraPosition -= right * time * 50f;
                if (keyboard.IsKeyDown(Keys.D))
                    cameraPosition += right * time * 50f;

                System.Diagnostics.Debug.WriteLine(cameraPosition);

                camera.View = Matrix.Invert(rotation * Matrix.CreateTranslation(cameraPosition));

                game.IsMouseVisible = false;
                Mouse.SetPosition((int)resolution.Value.X / 2, (int)resolution.Value.Y / 2);
            }
            else
                game.IsMouseVisible = true;

            //camera.View = spotLight.view;
            //camera.Projection = spotLight.projection;

            //var sunLight = sun.GetBehaviour<SunLight>();
            //camera.View = sunLight.shadowViewMatrices[0];
            //camera.Projection = sunLight.shadowProjectionMatrices[0];
            //camera.NearClip = 1;
            //camera.FarClip = sunLight.farClip[0];

            for (int i = 0; i < lights.Count / 10; i++)
            {
                var light = lights[i];
                var offset = i * (MathHelper.TwoPi / (lights.Count / 10));
                light.Position = new Vector3(
                    (float)Math.Cos(totalTime + offset) * 40,
                    10,
                    (float)Math.Sin(totalTime + offset) * 40);
            }

            for (int i = lights.Count / 10; i < lights.Count; i++)
            {
                var light = lights[i];
                var offset = i * (MathHelper.TwoPi / (lights.Count  - (lights.Count / 10)));
                light.Position = new Vector3(
                    (float)Math.Cos(-totalTime + offset) * 100,
                    10,
                    (float)Math.Sin(-totalTime + offset) * 100);
            }

            //spotLight.Position = new Vector3(
            //    (float)Math.Cos(-totalTime + 5) * 50,
            //    100,
            //    (float)Math.Sin(-totalTime + 5) * 50);
            //spotLight.Direction = Vector3.Normalize(-spotLight.Position);

            scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw(GameTime gameTime)
        {
            scene.Draw();

            //var sunLight = sun.GetBehaviour<SunLight>();
            //if (spotLight.shadowMap != null)
            //{
            //    sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            //    sb.Draw(spotLight.shadowMap, Vector2.Zero, Color.White);
            //    sb.End();
            //}

            //var quad = new Quad(sb.GraphicsDevice);
            //basic.Parameters["Colour"].SetValue(Color.White.ToVector4());
            //quad.Draw(basic);
        }
    }
}
