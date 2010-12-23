using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.StateManagement
{
    /// <summary>
    /// The state of a transition.
    /// </summary>
    public enum TransitionState
    {
        /// <summary>
        /// Hidden.
        /// </summary>
        Hidden,
        /// <summary>
        /// Shown.
        /// </summary>
        Shown,
        /// <summary>
        /// Transitioning on.
        /// </summary>
        On,
        /// <summary>
        /// Transitioning off.
        /// </summary>
        Off,
    }
}
