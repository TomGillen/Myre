using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Extensions;

namespace Myre.StateManagement
{
    /// <summary>
    /// A class which represents a transition.
    /// </summary>
    public class Transition
    {
        private TimeSpan onTime;
        private TimeSpan offTime;
        private float progress;
        private bool targettingOn;

        /// <summary>
        /// The time taken to transition from 0 to 1.
        /// </summary>
        public TimeSpan OnDuration
        {
            get { return onTime; }
            set
            {
                if (value.Ticks < 0)
                    throw new ArgumentOutOfRangeException("value", "Cannot be less than 0.");
                onTime = value;
            }
        }

        /// <summary>
        /// The time taken to transition from 1 to 0.
        /// </summary>
        public TimeSpan OffDuration
        {
            get { return offTime; }
            set
            {
                if (value.Ticks < 0)
                    throw new ArgumentOutOfRangeException("value", "Cannot be less than 0.");
                offTime = value;
            }
        }

        /// <summary>
        /// The state of the transition. Between 0 (off) and 1 (on).
        /// </summary>
        public float Progress
        {
            get { return progress; }
        }

        /// <summary>
        /// Creates a new instance of the Transition class.
        /// </summary>
        /// <param name="onDuration">The time taken to transition from 0 to 1.</param>
        /// <param name="offDuration">The time taken to transition from 1 to 0.</param>
        public Transition(TimeSpan onDuration, TimeSpan offDuration)
        {
            this.onTime = onDuration;
            this.offTime = offDuration;
        }

        /// <summary>
        ///  Creates a new instance of the Transition class.
        /// </summary>
        /// <param name="duration">The time taken to transition on or off.</param>
        public Transition(TimeSpan duration)
            : this(duration, duration)
        {
        }

        /// <summary>
        /// Updates the transition.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public void Update(GameTime gameTime)
        {
            var dt = (float)gameTime.Seconds();
            if (targettingOn)
            {
                if (onTime.Ticks == 0)
                    progress = 1;
                else
                    progress = MathHelper.Clamp(progress + (dt / (float)onTime.TotalSeconds), 0, 1);
            }
            else
            {
                if (offTime.Ticks == 0)
                    progress = 0;
                else
                    progress = MathHelper.Clamp(progress - (dt / (float)offTime.TotalSeconds), 0, 1);
            }
        }

        /// <summary>
        /// Makes the transition move towards 1.
        /// </summary>
        public void MoveOn()
        {
            targettingOn = true;
        }

        /// <summary>
        /// Makes the transition move towards 0.
        /// </summary>
        public void MoveOff()
        {
            targettingOn = false;
        }
    }
}
