﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the Microsoft.Xna.Framework.Graphics.GraphicsDevice class.
    /// </summary>
    public static class GraphicsDeviceExtensions
    {
        /// <summary>
        /// Calculates half the size of a pixel.
        /// </summary>
        /// <returns>Half the size of a pixel</returns>
        public static Vector2 CalculateHalfPixel(this GraphicsDevice device)
        {
            Vector2 resolution = device.GetResolution();
            return new Vector2(0.5f / resolution.X, 0.5f / resolution.Y);
        }

        /// <summary>
        /// Gets the resolution of the currently set render target or back buffer.
        /// </summary>
        /// <returns>The resolution of the currently set render target or back buffer.</returns>
        public static Vector2 GetResolution(this GraphicsDevice device)
        {
            PresentationParameters pp = device.PresentationParameters;
            return new Vector2(pp.BackBufferWidth, pp.BackBufferHeight);
        }

        /// <summary>
        /// Clears the depth buffer.
        /// </summary>
        /// <param name="device">The graphics device to clear.</param>
        /// <param name="depth">The depth to clear the depth buffer to.</param>
        public static void ClearDepth(this GraphicsDevice device, int depth)
        {
            device.Clear(ClearOptions.DepthBuffer, Color.White, depth, 0);
        }
    }
}
