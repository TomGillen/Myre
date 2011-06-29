using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.PostProcessing
{
    public class Gaussian
    {
        Effect effect;
        Quad quad;
        GraphicsDevice device;
        float[] weights;
        float[] offsets;

        int height;
        int width;
        float sigma;

        public Gaussian(GraphicsDevice device)
        {
            this.effect = Content.Load<Effect>("Gaussian");
            this.quad = new Quad(device);
            this.device = device;

            var sampleCount = effect.Parameters["Weights"].Elements.Count;
            this.weights = new float[sampleCount];
            this.offsets = new float[sampleCount];
        }

        public void Blur(RenderTarget2D source, RenderTarget2D destination, float sigma)
        {
            if (width != source.Width || height != source.Height || this.sigma != sigma)
                CalculateWeights(source.Width, source.Height, sigma);

            effect.Parameters["Resolution"].SetValue(new Vector2(width, height));

            var intermediate = RenderTargetManager.GetTarget(device, width, height, destination.Format, name:"gaussian intermediate");
            device.SetRenderTarget(intermediate);

            effect.Parameters["Texture"].SetValue(source);
            effect.CurrentTechnique = effect.Techniques["BlurHorizontal"];
            quad.Draw(effect);

            device.SetRenderTarget(destination);

            effect.Parameters["Texture"].SetValue(intermediate);
            effect.CurrentTechnique = effect.Techniques["BlurVertical"];
            quad.Draw(effect);

            RenderTargetManager.RecycleTarget(intermediate);
        }

        // from the bloom sample on creators.xna.com
        private void CalculateWeights(int width, int height, float sigma)
        {
            this.width = width;
            this.height = height;
            this.sigma = sigma;

            // The first sample always has a zero offset.
            weights[0] = ComputeGaussian(0);
            offsets[0] = 0;

            // Maintain a sum of all the weighting values.
            float totalWeights = weights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < weights.Length / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                weights[i * 2 + 1] = weight;
                weights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float offset = i * 2 + 1.5f;

                // Store texture coordinate offsets for the positive and negative taps.
                offsets[i * 2 + 1] = offset;
                offsets[i * 2 + 2] = -offset;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            effect.Parameters["Weights"].SetValue(weights);
            effect.Parameters["Offsets"].SetValue(offsets);
        }

        private float ComputeGaussian(float n)
        {
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * sigma)) *
                           Math.Exp(-(n * n) / (2 * sigma * sigma)));
        }
    }
}
