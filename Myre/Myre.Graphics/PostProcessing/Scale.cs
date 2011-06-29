using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Myre.Extensions;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.PostProcessing
{
    public class Resample
    {
        GraphicsDevice device;
        Effect effect;
        Quad quad;

        public Resample(GraphicsDevice device)
        {
            this.device = device;
            this.effect = Content.Load<Effect>("Downsample");
            this.quad = new Quad(device);
        }

        public void Scale(RenderTarget2D source, RenderTarget2D destination)
        {
            effect.CurrentTechnique = source.Format.IsFloatingPoint() ? effect.Techniques["Software"] : effect.Techniques["Hardware"];

            Vector2 resolution = new Vector2(source.Width, source.Height);
            float ScaleFactor = (destination.Width > source.Width) ? 2 : 0.5f;

            RenderTarget2D input = source;
            RenderTarget2D output = null;

            while (IntermediateNeeded(resolution, destination, ScaleFactor))
            {
                resolution *= ScaleFactor;

                output = RenderTargetManager.GetTarget(device, (int)resolution.X, (int)resolution.Y, source.Format, name:"scaled");
                Draw(input, output);

                if (input != source)
                    RenderTargetManager.RecycleTarget(input);
                input = output;
            }

            Draw(input, destination);

            if (input != source)
                RenderTargetManager.RecycleTarget(input);
        }

        private bool IntermediateNeeded(Vector2 currentResolution, RenderTarget2D target, float scale)
        {
            return (scale == 2) ? (currentResolution.X * 2 < target.Width && currentResolution.Y * 2 < target.Height)
                                : (currentResolution.X / 2 > target.Width && currentResolution.Y / 2 > target.Height);
        }

        private void Draw(RenderTarget2D input, RenderTarget2D output)
        {
            device.SetRenderTarget(output);

            effect.Parameters["Resolution"].SetValue(new Vector2(output.Width, output.Height));
            effect.Parameters["SourceResolution"].SetValue(new Vector2(input.Width, input.Height));
            effect.Parameters["Texture"].SetValue(input);

            quad.Draw(effect);
        }
    }
}
