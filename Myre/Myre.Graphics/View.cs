using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Myre.Graphics
{
    [DefaultManager(typeof(Manager))]
    public class View
        : Behaviour
    {
        private Property<Camera> camera;
        private Property<Viewport> viewport;

        public Camera Camera
        {
            get { return camera.Value; }
            set { camera.Value = value; }
        }

        public Viewport Viewport
        {
            get { return viewport.Value; }
            set { viewport.Value = value; }
        }

        public override void Initialise(Entity.InitialisationContext context)
        {
            this.camera = context.GetOrCreateProperty<Camera>("camera");
            this.viewport = context.GetOrCreateProperty<Viewport>("viewport");
            base.Initialise(context);
        }

        /*
        public Property<Camera> Camera { get; private set; }
        public Property<Viewport> Viewport { get; private set; }

        public View(
            Property<Camera> camera,
            Property<Viewport> viewport)
        {
            this.Camera = camera;
            this.Viewport = viewport;
        }
        */

        public void SetMetadata(RendererMetadata metadata)
        {
            metadata.Set("activeview", this);
            metadata.Set("resolution", new Vector2(viewport.Value.Width, viewport.Value.Height));
            metadata.Set("viewport", viewport.Value);
            camera.Value.SetMetadata(metadata);
        }


        public class Manager
            : BehaviourManager<View>
        {
            private Renderer renderService;

            public Manager(
                [SceneService] Renderer renderer)
            {
                this.renderService = renderer;
            }

            public override void Add(View behaviour)
            {
                base.Add(behaviour);
                renderService.Views.Add(behaviour);
            }

            public override bool Remove(View behaviour)
            {
                renderService.Views.Remove(behaviour);
                return base.Remove(behaviour);
            }
        }
    }
}
