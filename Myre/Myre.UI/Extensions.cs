using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Myre.UI
{
    /// <summary>
    /// A static class containing extension methods.
    /// </summary>
    static class Extensions
    {
        #region Vector2
        /// <summary>
        /// Converts this Vector2 into an Int2D.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        public static Int2D ToInt2D(this Vector2 v)
        {
            return new Int2D(v);
        }
        #endregion
    }
}
