using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Particles
{
    public class ParticlePhase
        : RendererComponent
    {
        //private Model tank;
        private ReadOnlyCollection<ParticleEmitter.Manager> managers;

        public ParticlePhase(ContentManager content)
        {
            //tank = content.Load<Model>("tank");
        }

        public override void Initialise(Renderer renderer)
        {
            managers = renderer.Scene.FindManagers<ParticleEmitter.Manager>();
            base.Initialise(renderer);
        }

        protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? output)
        {
            inputs.Add(new Input() { Name = "gbuffer_depth", Optional = true });
            output = null;
        }

        protected internal override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
        {
            if (previousRenderTarget == null)
                return false;

            if (previousRenderTarget.Value.DepthFormat == DepthFormat.None)
                return false;

            return true;
        }

        public override RenderTarget2D Draw(Renderer renderer)
        {
            //DrawModel(renderer, tank);

            foreach (var item in managers)
                item.Draw(renderer);

            return null;
        }

        ///// <summary>
        ///// Helper for drawing the spinning 3D model.
        ///// </summary>
        //void DrawModel(Renderer renderer, Model model)
        //{
        //    renderer.Device.DepthStencilState = DepthStencilState.Default;
        //    renderer.Device.BlendState = BlendState.Opaque;
        //    renderer.Device.RasterizerState = RasterizerState.CullCounterClockwise;

        //    Viewport viewport = renderer.Data.Get<Viewport>("viewport").Value;
        //    float aspectRatio = (float)viewport.Width / (float)viewport.Height;

        //    // Create camera matrices.
        //    Matrix world = Matrix.Identity;

        //    Matrix view = renderer.Data.Get<Matrix>("view").Value;

        //    Matrix projection = renderer.Data.Get<Matrix>("projection").Value;

        //    // Look up the bone transform matrices.
        //    Matrix[] transforms = new Matrix[model.Bones.Count];

        //    model.CopyAbsoluteBoneTransformsTo(transforms);

        //    // Draw the model.
        //    foreach (ModelMesh mesh in model.Meshes)
        //    {
        //        foreach (BasicEffect effect in mesh.Effects)
        //        {
        //            effect.World = transforms[mesh.ParentBone.Index] * world;
        //            effect.View = view;
        //            effect.Projection = projection;

        //            effect.EnableDefaultLighting();

        //            // Override the default specular color to make it nice and bright,
        //            // so we'll get some decent glints that the bloom can key off.
        //            effect.SpecularColor = Vector3.One;
        //        }

        //        mesh.Draw();
        //    }
        //}
    }
}
