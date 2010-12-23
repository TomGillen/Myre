﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Myre.Debugging.Statistics;
using Myre.Collections;
using Myre.Graphics.PostProcessing;

namespace Myre.Graphics.Lighting
{
    public class ToneMapPhase
       : RendererComponent
    {
        protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
        {
            inputs.Add(new Input() { Name = "lightbuffer" });
            outputs.Add(new Resource() { Name = "luminancemap" });
            outputs.Add(new Resource() { Name = "bloom" });
            outputs.Add(new Resource() { Name = "tonemapped", IsLeftSet = true });
            outputs.Add(new Resource() { Name = "luminance" });

            outputTarget = new RenderTargetInfo()
            {
                SurfaceFormat = SurfaceFormat.Rgba64
            };
        }

        protected internal override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
        {
            return true;
        }

        Quad quad;
        Material calculateLuminance;
        Material adaptLuminance;
        Material toneMap;
        Effect bloom;
        Gaussian gaussian;
        Resample scale;
        RenderTarget2D[] adaptedLuminance;
        float[] textureData = new float[1];
        int current = 0;
        int previous = 1;

        public RenderTarget2D AdaptedLuminance
        {
            get { return adaptedLuminance[previous]; }
        }

        public ToneMapPhase(
            GraphicsDevice device,
            ContentManager content)
        {
            quad = new Quad(device);
            var effect = content.Load<Effect>("CalculateLuminance");
            calculateLuminance = new Material(effect.Clone(), "ExtractLuminance");
            adaptLuminance = new Material(effect.Clone(), "AdaptLuminance");
            toneMap = new Material(content.Load<Effect>("ToneMap"), null);
            bloom = content.Load<Effect>("Bloom");
            gaussian = new Gaussian(device, content);
            scale = new Resample(device, content);

            adaptedLuminance = new RenderTarget2D[2];
            adaptedLuminance[0] = new RenderTarget2D(device, 1, 1, false, SurfaceFormat.Single, DepthFormat.None);
            adaptedLuminance[1] = new RenderTarget2D(device, 1, 1, false, SurfaceFormat.Single, DepthFormat.None);

            device.SetRenderTarget(adaptedLuminance[previous]);
            device.Clear(Color.Transparent);
            device.SetRenderTarget(null);
        }

        public override void Initialise(Renderer renderer)
        {
            var settings = renderer.Settings;

            settings.Add("hdr_adaptionrate", "The rate at which the cameras' exposure adapts to changes in the scene luminance.", 3f);
            settings.Add("hdr_bloomthreshold", "The under-exposure applied during bloom thresholding.", 1f);
            settings.Add("hdr_bloommagnitude", "The overall brightness of the bloom effect.", 0.5f);
            settings.Add("hdr_bloomblurammount", "The ammount to blur the bloom target.", 2.2f);
            
            base.Initialise(renderer);
        }

        public override RenderTarget2D Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var lightBuffer = renderer.GetResource("lightbuffer");
            var resolution = metadata.Get<Vector2>("resolution");

            CalculateLuminance(renderer, resolution, device, lightBuffer);
            Bloom(renderer, resolution, device, lightBuffer);
            return ToneMap(renderer, resolution, device, lightBuffer);
        }

        private void CalculateLuminance(Renderer renderer, Box<Vector2> resolution, GraphicsDevice device, RenderTarget2D lightBuffer)
        {
            var tmp = previous;
            previous = current;
            current = tmp;

            // calculate luminance map
            var luminanceMap = RenderTargetManager.GetTarget(device, 1024, 1024, SurfaceFormat.Single, mipMap: false);
            device.SetRenderTarget(luminanceMap);
            device.BlendState = BlendState.Opaque;
            device.Clear(Color.Transparent);
            calculateLuminance.Parameters["Texture"].SetValue(lightBuffer);
            quad.Draw(calculateLuminance, renderer.Data);
            renderer.SetResource("luminancemap", luminanceMap);

            luminanceMap = DownsampleLuminance(renderer, luminanceMap);

            // adapt towards the current luminance
            device.SetRenderTarget(adaptedLuminance[current]);
            adaptLuminance.Parameters["Texture"].SetValue(luminanceMap);
            adaptLuminance.Parameters["PreviousLuminance"].SetValue(adaptedLuminance[previous]);
            var oldResolution = resolution.Value;
            resolution.Value = Vector2.One;
            quad.Draw(adaptLuminance, renderer.Data);

            resolution.Value = oldResolution;

            // retrieve the previous frames luminance
            adaptedLuminance[previous].GetData<float>(
               adaptedLuminance[previous].LevelCount - 1,
               new Rectangle(0, 0, 1, 1),
               textureData,
               0,
               1);
            renderer.Data.Set<float>("adaptedluminance", textureData[0]);
        }

        private RenderTarget2D DownsampleLuminance(Renderer renderer, RenderTarget2D luminanceMap)
        {
            var downsampled = RenderTargetManager.GetTarget(renderer.Device, 1, 1, luminanceMap.Format);
            scale.Scale(luminanceMap, downsampled);
            renderer.SetResource("luminance", downsampled);

            return downsampled;
        }

        private void Bloom(Renderer renderer, Box<Vector2> resolution, GraphicsDevice device, RenderTarget2D lightBuffer)
        {
            var screenResolution = resolution.Value;
            var halfResolution = screenResolution / 2;
            var quarterResolution = halfResolution / 2;

            // downsample the light buffer to half resolution, and threshold at the same time
            var thresholded = RenderTargetManager.GetTarget(device, (int)halfResolution.X, (int)halfResolution.Y, SurfaceFormat.Rgba64);
            device.SetRenderTarget(thresholded);
            bloom.Parameters["Resolution"].SetValue(halfResolution);
            bloom.Parameters["Threshold"].SetValue(renderer.Data.Get<float>("hdr_bloomthreshold").Value);
            bloom.Parameters["Texture"].SetValue(lightBuffer);
            bloom.Parameters["Luminance"].SetValue(adaptedLuminance[current]);
            bloom.CurrentTechnique = bloom.Techniques["ThresholdDownsample2X"];
            quad.Draw(bloom);

            // downsample again to quarter resolution
            var downsample = RenderTargetManager.GetTarget(device, (int)quarterResolution.X, (int)quarterResolution.Y, SurfaceFormat.Rgba64);
            device.SetRenderTarget(downsample);
            bloom.Parameters["Resolution"].SetValue(quarterResolution);
            bloom.Parameters["Texture"].SetValue(thresholded);
            bloom.CurrentTechnique = bloom.Techniques["Scale"];
            quad.Draw(bloom);

            // blur the target
            var blurred = RenderTargetManager.GetTarget(device, (int)quarterResolution.X, (int)quarterResolution.Y, SurfaceFormat.Rgba64);
            gaussian.Blur(downsample, blurred, renderer.Data.Get<float>("hdr_bloomblurammount").Value);

            // upscale back to half resolution
            device.SetRenderTarget(thresholded);
            bloom.Parameters["Resolution"].SetValue(halfResolution);
            bloom.Parameters["Texture"].SetValue(blurred);
            quad.Draw(bloom);

            // output result
            renderer.SetResource("bloom", thresholded);

            // cleanup temp render targets
            RenderTargetManager.RecycleTarget(downsample);
            RenderTargetManager.RecycleTarget(blurred);
        }

        private RenderTarget2D ToneMap(Renderer renderer, Box<Vector2> resolution, GraphicsDevice device, RenderTarget2D lightBuffer)
        {
            var toneMapped = RenderTargetManager.GetTarget(device, (int)resolution.Value.X, (int)resolution.Value.Y, SurfaceFormat.Rgba64);
            device.SetRenderTarget(toneMapped);
            toneMap.Parameters["Texture"].SetValue(lightBuffer);
            toneMap.Parameters["Luminance"].SetValue(adaptedLuminance[current]);
            quad.Draw(toneMap, renderer.Data);
            renderer.SetResource("tonemapped", toneMapped);

            return toneMapped;
        }

        #region Tone Mapping Math Functions
        public static Vector3 ToneMap(Vector3 colour, float adaptedLuminance)
        {
            return ToneMapFilmic(CalcExposedColor(colour, adaptedLuminance));
        }

        // From http://mynameismjp.wordpress.com/2010/04/30/a-closer-look-at-tone-mapping
        // Applies the filmic curve from John Hable's presentation
        private static Vector3 ToneMapFilmic(Vector3 color)
        {
            color = Vector3.Max(Vector3.Zero, color - new Vector3(0.004f));
            color = (color * (6.2f * color + new Vector3(0.5f))) / (color * (6.2f * color + new Vector3(1.7f)) + new Vector3(0.06f));

            return color;
        }

        public static Vector3 InverseToneMap(Vector3 colour, float adaptedLuminance)
        {
            return InverseExposedColour(InverseToneMapFilmic(colour), adaptedLuminance);
        }

        private static Vector3 InverseToneMapFilmic(Vector3 colour)
        {
            return new Vector3(
                InverseToneMapFilmic(colour.X),
                InverseToneMapFilmic(colour.Y),
                InverseToneMapFilmic(colour.Z));
        }

        private static float InverseToneMapFilmic(float x)
        {
            var numerator = Math.Sqrt(5)
                * Math.Sqrt(701 * x * x - 106 * x + 125) - 856 * x + 25;
            var denumerator = 620 * (x - 1);

            return (float)Math.Abs(numerator / denumerator) + 0.004f;
        }

        // Determines the color based on exposure settings
        public static Vector3 CalcExposedColor(Vector3 color, float avgLuminance)
        {
            // Use geometric mean        
            avgLuminance = Math.Max(avgLuminance, 0.001f);

            float keyValue = 1.03f - (2.0f / (2 + (float)Math.Log10(avgLuminance + 1)));

            float linearExposure = (keyValue / avgLuminance);
            float exposure = Math.Max(linearExposure, 0.0001f);

            return exposure * color;
        }

        public static Vector3 InverseExposedColour(Vector3 colour, float avgLuminance)
        {
            avgLuminance = Math.Max(avgLuminance, 0.001f);

            float keyValue = 1.03f - (2.0f / (2 + (float)Math.Log10(avgLuminance + 1)));

            float linearExposure = (keyValue / avgLuminance);
            float exposure = Math.Max(linearExposure, 0.0001f);

            return colour / exposure;
        }
        #endregion
    }
}