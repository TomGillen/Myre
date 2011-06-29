﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Deferred
{
    public class RestoreDepthPhase
        : RendererComponent
    {
        private Quad quad;
        private Material restoreDepth;

        public bool ClearDepth { get; set; }

        public RestoreDepthPhase(GraphicsDevice device)
        {
            this.quad = new Quad(device);
            this.restoreDepth = new Material(Content.Load<Effect>("RestoreDepth").Clone());
            this.ClearDepth = true;
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define inputs
            context.DefineInput("gbuffer_depth");

            // define outputs
            foreach (var resource in context.SetRenderTargets)
                context.DefineOutput(resource);
            
            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            // work arround for a bug in xna 4.0
            renderer.Device.SamplerStates[0] = SamplerState.LinearClamp;
            renderer.Device.SamplerStates[0] = SamplerState.PointClamp;

            if (ClearDepth)
                renderer.Device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1, 0);

            renderer.Device.DepthStencilState = DepthStencilState.Default;
            renderer.Device.BlendState = BlendState.Additive;
            quad.Draw(restoreDepth, renderer.Data);
        }
    }
}
