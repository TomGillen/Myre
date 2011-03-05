using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Myre;
using Myre.StateManagement;
using Myre.Debugging.Statistics;
using Myre.UI;
using Ninject;
using Myre.Graphics.Geometry;
using Myre.Collections;
using Myre.Entities;
using Myre.Graphics;
using Myre.Entities.Ninject;
using Ninject.Planning.Bindings.Resolvers;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Myre.UI.InputDevices;
using Myre.Debugging;

namespace GraphicsTests
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TestGame : NinjectGame
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ScreenManager screens;

        UserInterface ui;
        Statistic frameTime;
        FrequencyTracker fps;

        public TestGame()
            : base(new EntityNinjectModule())
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            graphics.PreferredBackBufferWidth = 1920; //1280;
            graphics.PreferredBackBufferHeight = 1080;// 720;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.IsFullScreen = false;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            Kernel.Components.Add<IBindingResolver, ConditionalBindingResolver>();
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            foreach (GraphicsAdapter adapter in GraphicsAdapter.Adapters)
            {
                if (adapter.Description.Contains("PerfHUD"))
                {
                    e.GraphicsDeviceInformation.Adapter = adapter;
                    GraphicsAdapter.UseReferenceDevice = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ui = new UserInterface(GraphicsDevice);
            Components.Add(ui);

            var player = new InputActor(1, new MouseDevice(), new KeyboardDevice(PlayerIndex.One, Window.Handle));
            Kernel.Bind<InputActor>().ToConstant(player);
            Components.Add(player);
            ui.Actors.Add(player);

            var statLog = new StatisticTextLog(ui.Root, Content.Load<SpriteFont>("Consolas"), true);
            statLog.SetPoint(Points.TopLeft, 10, 10);

            frameTime = Statistic.Get("Misc.Time", "{0:00.00}ms");
            fps = new FrequencyTracker("Misc.FPS");

            var console = new CommandConsole(this, Content.Load<SpriteFont>("Consolas"), ui.Root);
            Kernel.Bind<CommandConsole>().ToConstant(console);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            screens = new ScreenManager();
            screens.Push(Kernel.Get<MainMenu>());            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            fps.Pulse();
            frameTime.Value = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            //// TODO: Add your update logic here
            screens.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //// TODO: Add your drawing code here
            screens.PrepareDraw();
            screens.Draw(gameTime);

            //var model = Content.Load<Model>("Sponza");

            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            //GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            //GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            //GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            //// Calculate aspect ratio
            //float aspectRatio = (float)GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height;

            //// Animated the model rotating
            //float modelRotation = (float)gameTime.TotalGameTime.TotalSeconds / 5.0f;

            //// Set the positions of the camera in world space, for our view matrix.
            //Vector3 cameraPosition = new Vector3(0.0f, 50.0f, -200.0f);
            //Vector3 lookAt = new Vector3(0.0f, 40.0f, 300.0f);

            //// Copy any parent transforms.
            //Matrix[] transforms = new Matrix[model.Bones.Count];
            //model.CopyAbsoluteBoneTransformsTo(transforms);

            //// Draw the model. A model can have multiple meshes, so loop.
            //foreach (ModelMesh mesh in model.Meshes)
            //{
            //    // This is where the mesh orientation is set,
            //    // as well as our camera and projection.
            //    foreach (BasicEffect effect in mesh.Effects)
            //    {
            //        effect.EnableDefaultLighting();
            //        effect.World =
            //            Matrix.CreateScale(1) *
            //            transforms[mesh.ParentBone.Index] *
            //            Matrix.CreateRotationY(modelRotation);
            //        effect.View = Matrix.CreateLookAt(cameraPosition, lookAt,
            //            Vector3.Up);
            //        effect.Projection = Matrix.CreatePerspectiveFieldOfView(
            //            MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
            //    }
            //    // Draw the mesh, using the effects set above.
            //    mesh.Draw();
            //}

            base.Draw(gameTime);
        }
    }
}
