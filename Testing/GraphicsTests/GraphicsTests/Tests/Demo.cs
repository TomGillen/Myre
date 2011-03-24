using System;
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

        //private AviManager aviManager;
        //private VideoStream aviStream;
        //private DefaultTimeline timeline;
        //private IGroup videoGroup;
        //private ITrack videoTrack;
        //private int framesPerSecond = 60;
        //private int width = 1280;
        //private int height = 720;
        private float time;

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
            renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<EdgeDetectComponent>()
                .Then<Ssao>()
                .Then<LightingComponent>()
                .Then<ToneMapPhase>()
                //.Then<RestoreDepthPhase>()
                .Then<ParticleComponent>()
                //.Then<AntiAliasComponent>()
                .Apply();

            base.OnShown();

            //var game = kernel.Get<TestGame>();
            game.DisplayUI = true;
            //game.IsFixedTimeStep = true;
        }

        public override void OnHidden()
        {
            //aviStream.Close();
            //aviManager.Close();

            //IRenderer renderer = new WindowsMediaRenderer(timeline, "demo.wmv", WindowsMediaProfiles.HighQualityVideo);
            //IAsyncResult result = renderer.BeginRender(null, null);
            //renderer.EndRender(result);

            base.OnHidden();
        }

        public override void Update(GameTime gameTime)
        {
            scene.Update(gameTime);
            base.Update(gameTime);

            time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (time >= 40)
                Manager.Pop();
        }

        public override void Draw(GameTime gameTime)
        {
            scene.Draw(gameTime);
            base.Draw(gameTime);

            //var pp = game.GraphicsDevice.PresentationParameters;
            ////var data = new Color[pp.BackBufferWidth * pp.BackBufferHeight];
            ////game.GraphicsDevice.GetBackBufferData(data);

            //byte[] textureData = new byte[4 * pp.BackBufferWidth * pp.BackBufferHeight];
            //game.GraphicsDevice.GetBackBufferData<byte>(textureData);
            
            //Bitmap bmp = new System.Drawing.Bitmap(pp.BackBufferWidth, pp.BackBufferHeight, PixelFormat.Format32bppArgb);
            //BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, pp.BackBufferWidth, pp.BackBufferHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            //IntPtr safePtr = bmpData.Scan0;
            //Marshal.Copy(textureData, 0, safePtr, textureData.Length); 
            //bmp.UnlockBits(bmpData);

            ////if (aviStream == null)
            ////    aviStream = aviManager.AddVideoStream(false, 60, bmp);
            ////else
            ////    aviStream.AddFrame(bmp);

            //videoTrack.AddImage(bmp, time, time + 1f / framesPerSecond);

            //bmp.Dispose();
        }
    }
}
