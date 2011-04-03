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
using Microsoft.Xna.Framework.Input;
using Myre.UI.Gestures;
using Myre.Graphics.Particles;
using Myre.Collections;

namespace GraphicsTests.Tests
{
    class Demo
        : TestScreen
    {
        private IKernel kernel;
        private ContentManager content;
        private GraphicsDevice device;
        private TestScene scene;
        private TestGame game;

        private Box<float> ssaoIntensity;
        private RenderPlan fullPlan;
        private RenderPlan ssaoPlan;
        private RenderPlan lightingPlan;
        private RenderPlan edgeDetectPlan;

        public Demo(
            IKernel kernel,
            TestGame game,
            ContentManager content,
            GraphicsDevice device)
            : base("Demo", kernel)
        {
            this.kernel = kernel;
            this.content = content;
            this.device = device;
            this.game = game;

            //aviManager = new AviManager(@"demo.avi", false);

            //timeline = new DefaultTimeline(framesPerSecond);
            //timeline.AddAudioGroup("main");
            //videoGroup = timeline.AddVideoGroup("main", framesPerSecond, 32, width, height);
            //videoTrack = videoGroup.AddTrack();
        }

        public override void OnShown()
        {
            scene = kernel.Get<TestScene>();

            var renderer = scene.Scene.GetService<Renderer>();

            ssaoIntensity = renderer.Data.Get<float>("ssao_intensity");
            
            fullPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<EdgeDetectComponent>()
                .Then<Ssao>()
                .Then<LightingComponent>()
                .Then<ToneMapComponent>()
                .Then<ParticleComponent>();

            ssaoPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<Ssao>()
                .Show("ssao");

            lightingPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<Ssao>()
                .Then<LightingComponent>()
                .Then<ParticleComponent>();
                //.Show("lightbuffer");

            edgeDetectPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<EdgeDetectComponent>()
                .Show("edges");

            fullPlan.Apply();

            base.OnShown();

            //var game = kernel.Get<TestGame>();
            game.DisplayUI = true;
            //game.IsFixedTimeStep = true;
        }

        public override void OnHidden()
        {
            base.OnHidden();
        }

        public override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.D1))
                ssaoPlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D3))
                edgeDetectPlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D4))
                lightingPlan.Apply();
            else
                fullPlan.Apply();

            if (keyboard.IsKeyDown(Keys.D2))
                ssaoIntensity.Value = 0;
            else
                ssaoIntensity.Value = 20;

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
