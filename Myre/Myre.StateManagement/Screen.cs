﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.StateManagement
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Screen
    {
        TransitionState transitionState;

        /// <summary>
        /// Gets the <see cref="ScreenManager"/> which this screen was last pushed onto.
        /// </summary>
        /// <value>The manager.</value>
        public ScreenManager Manager { get; internal set; }

        /// <summary>
        /// Updates the screen.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public virtual void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Draws this instance.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public virtual void Draw(GameTime gameTime)
        {
        }

        /// <summary>
        /// Prepares the draw.
        /// </summary>
        public virtual void PrepareDraw()
        {
        }

        /// <summary>
        /// Gets or sets the transition on time.
        /// </summary>
        /// <value>The transition on.</value>
        public TimeSpan TransitionOn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the transition off time.
        /// </summary>
        /// <value>The transition off.</value>
        public TimeSpan TransitionOff
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or transition progress.
        /// 0 is hidden, and 1 is shown.
        /// </summary>
        /// <value>The transition progress.</value>
        public float TransitionProgress
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the state of the transition.
        /// </summary>
        /// <value>The state of the transition.</value>
        public TransitionState TransitionState
        {
            get { return transitionState; }
            internal set
            {
                if (transitionState != value)
                {
                    transitionState = value;
                    if (transitionState == TransitionState.Hidden)
                        OnHidden();
                    else
                        OnShown();
                }
            }
        }

        /// <summary>
        /// Called when the screen becomes visible.
        /// </summary>
        public virtual void OnShown()
        {
        }

        /// <summary>
        /// Called when the screen becomes hidden.
        /// </summary>
        public virtual void OnHidden()
        {
        }
    }
}
