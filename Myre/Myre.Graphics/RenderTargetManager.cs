using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Myre.Debugging.Statistics;
using Myre.Extensions;

namespace Myre.Graphics
{
    public struct RenderTargetInfo
    {
        public int Height;
        public int Width;
        public SurfaceFormat SurfaceFormat;
        public DepthFormat DepthFormat;
        public int MultiSampleCount;
        public bool MipMap;
        public RenderTargetUsage Usage;
        
        public bool Equals(RenderTargetInfo info)
        {
            return this.Height == info.Height
                && this.Width == info.Width
                && this.SurfaceFormat == info.SurfaceFormat
                && this.DepthFormat == info.DepthFormat
                && this.MultiSampleCount == info.MultiSampleCount
                && this.MipMap == info.MipMap
                && this.Usage == info.Usage;
        }

        public override bool Equals(object obj)
        {
            if (obj is RenderTargetInfo)
                return Equals((RenderTargetInfo)obj);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode()
                ^ Height
                ^ Width
                ^ SurfaceFormat.GetHashCode();
        }

        public static RenderTargetInfo FromRenderTarget(RenderTarget2D target)
        {
            return new RenderTargetInfo()
            {
                Height = target.Height,
                Width = target.Width,
                SurfaceFormat = target.Format,
                DepthFormat = target.DepthStencilFormat,
                MultiSampleCount = target.MultiSampleCount,
                MipMap = target.LevelCount > 1,
                Usage = target.RenderTargetUsage
            };
        }
    }

    public static class RenderTargetManager
    {
#if PROFILE
        private static Statistic numRenderTargets = Statistic.Get("Graphics.RTs");
        private static Statistic renderTargetMemory = Statistic.Get("Graphics.RT_Memory", "{0:0.00}MB");
#endif

        private static Dictionary<RenderTargetInfo, Stack<RenderTarget2D>> pool = new Dictionary<RenderTargetInfo, Stack<RenderTarget2D>>();
        private static Dictionary<RenderTargetInfo, RenderTargetInfo> infoMappings = new Dictionary<RenderTargetInfo, RenderTargetInfo>();


#if DEBUG
        private static List<string> active = new List<string>();
#endif

        public static RenderTarget2D GetTarget(GraphicsDevice device, int width, int height, SurfaceFormat surfaceFormat = SurfaceFormat.Color, DepthFormat depthFormat = DepthFormat.None, int multiSampleCount = 0, bool mipMap = false, RenderTargetUsage usage = RenderTargetUsage.DiscardContents, string name = null)
        {
            var info = new RenderTargetInfo()
            {
                Width = width,
                Height = height,
                SurfaceFormat = surfaceFormat,
                DepthFormat = depthFormat,
                MultiSampleCount = multiSampleCount,
                MipMap = mipMap,
                Usage = usage
            };

            return GetTarget(device, info, name);
        }

        public static RenderTarget2D GetTarget(GraphicsDevice device, RenderTargetInfo info, string name = null)
        {
            RenderTargetInfo mapped;
            bool wasMapped = infoMappings.TryGetValue(info, out mapped);
            if (!wasMapped)
                mapped = info;

            var stack = GetPool(mapped);
            if (stack.Count > 0)
            {
                var t = stack.Pop();
                t.Tag = name;
#if DEBUG
                active.Add(t.Tag as string);
#endif
                return t;
            }

            var target = new RenderTarget2D(device, mapped.Width, mapped.Height, mapped.MipMap, mapped.SurfaceFormat, mapped.DepthFormat, mapped.MultiSampleCount, mapped.Usage);
            target.Tag = name;

            if (!wasMapped)
            {
                var targetInfo = RenderTargetInfo.FromRenderTarget(target);
                infoMappings[info] = targetInfo;
            }

#if PROFILE
            numRenderTargets.Value++;

            var resolution = target.Width * target.Height;
            float size = resolution * target.Format.FormatSize();
            if (target.MultiSampleCount > 0)
                size *= target.MultiSampleCount;
            if (info.MipMap)
                size *= 1.33f;
            size += resolution * target.DepthStencilFormat.FormatSize();
            renderTargetMemory.Value += size / (1024 * 1024);
#endif

#if DEBUG
            active.Add(target.Tag as string);
#endif

            return target;
        }

        public static void RecycleTarget(RenderTarget2D target)
        {
            var info = RenderTargetInfo.FromRenderTarget(target);

#if DEBUG
            if (GetPool(info).Contains(target))
                throw new InvalidOperationException("Render target has already been freed.");

            active.Remove(target.Tag as string);
#endif

            GetPool(info).Push(target);
            target.Tag = null;
        }

        private static Stack<RenderTarget2D> GetPool(RenderTargetInfo info)
        {
            Stack<RenderTarget2D> stack;
            if (!pool.TryGetValue(info, out stack))
            {
                stack = new Stack<RenderTarget2D>();
                pool.Add(info, stack);
            }

            return stack;
        }
    }
}
