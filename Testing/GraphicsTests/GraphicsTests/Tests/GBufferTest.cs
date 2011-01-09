﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Ninject;
using Myre.Entities;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Myre.Graphics.Geometry;
using System.IO;
using Myre.Graphics.Lighting;

namespace GraphicsTests.Tests
{
    class GBufferTest
    : TestScreen
    {
        class Phase
            : RendererComponent
        {
            private SpriteBatch batch;

            public Phase(GraphicsDevice device)
            {
                batch = new SpriteBatch(device);
            }

            protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
            {
                inputs.Add(new Input() { Name = "gbuffer_depth" });
                inputs.Add(new Input() { Name = "gbuffer_depth_downsample" });
                inputs.Add(new Input() { Name = "gbuffer_normals" });
                inputs.Add(new Input() { Name = "gbuffer_diffuse" });
                outputs.Add(new Resource() { Name = "scene", IsLeftSet = true });

                outputTarget = new RenderTargetInfo();
            }

            protected override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
            {
                return true;
            }

            public override RenderTarget2D Draw(Renderer renderer)
            {
                var metadata = renderer.Data;
                var resolution = renderer.Data.Get<Vector2>("resolution").Value;
                var targetInfo = new RenderTargetInfo() { Width = (int)resolution.X, Height = (int)resolution.Y };
                var target = RenderTargetManager.GetTarget(renderer.Device, targetInfo);
                renderer.Device.SetRenderTarget(target);

                var depth = metadata.Get<Texture2D>("gbuffer_depth").Value;
                var normals = metadata.Get<Texture2D>("gbuffer_normals").Value;
                var diffuse = metadata.Get<Texture2D>("gbuffer_diffuse").Value;

                //Save(depth, "depth.jpg");
                //Save(normals, "normal.jpg");
                //Save(diffuse, "diffuse.jpg");

                var halfWidth = (int)(resolution.X / 2);
                var halfHeight = (int)(resolution.Y / 2);

                batch.GraphicsDevice.Clear(Color.Black);
                batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

                batch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                batch.Draw(depth, new Rectangle(0, 0, halfWidth, halfHeight), Color.White);
                batch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                batch.Draw(normals, new Rectangle(halfWidth, 0, halfWidth, halfHeight), Color.White);
                batch.Draw(diffuse, new Rectangle(0, halfHeight, halfWidth, halfHeight), Color.White);
                batch.End();

                renderer.SetResource("scene", target);
                return target;
            }

            private void Save(Texture2D texture, string name)
            {
                using (Stream stream = File.Create(name))
                    texture.SaveAsJpeg(stream, texture.Width, texture.Height);
            }
        }


        private IKernel kernel;
        private ContentManager content;
        private GraphicsDevice device;
        private TestScene scene;

        public GBufferTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Geometry Buffer", kernel)
        {
            this.kernel = kernel;
            this.content = content;
            this.device = device;
        }

        public override void OnShown()
        {
            scene = kernel.Get<TestScene>();

            var plan = RenderPlan
                .StartWith<GeometryBufferComponent>(kernel)
                .Then<Phase>();

            scene.Scene.GetService<Renderer>().Plan = plan;

            base.OnShown();
        }

        public override void Update(GameTime gameTime)
        {
            scene.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            scene.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
