﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the Microsoft.Xna.Framework.GameTime class.
    /// </summary>
    public static class GameTimeExtensions
    {
        /// <summary>
        /// Gets the number of seconds elapsed since the last frame.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns>The number of seconds elapsed since the last frame.</returns>
        public static float Seconds(this GameTime t)
        {
            return (float)t.ElapsedGameTime.TotalSeconds;
        }

        /// <summary>
        /// Gets the total number of seconds elapsed since the game started.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns>The total number of seconds elapsed since the game started.</returns>
        public static float TotalSeconds(this GameTime t)
        {
            return (float)t.TotalGameTime.TotalSeconds;
        }
    }
}
