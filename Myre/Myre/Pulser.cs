using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre
{
    /// <summary>
    /// Specifies the type of timing a pulsor uses.
    /// </summary>
    public enum PulserType
    {
        /// <summary>
        /// If more time has elapsed than the threashold since the last pulse, then the pulsor pulses and then resets the timer.
        /// </summary>
        Simple,

        /// <summary>
        /// The pulsor will ensure that it pulses the correct number of times in a perticular time interval.
        /// e.g. If the pulsor has a frequency of 30Hz, and it is not updated for 0.5 seconds, then its next update will pulse 15 times.
        /// </summary>
        Reliable,

        /// <summary>
        /// Same as reliable, except that the pulsor toggles between on and off at each 'pulse'.
        /// </summary>
        SquareWave
    }

    /// <summary>
    /// A class which implements a repeated timed event.
    /// </summary>
    public class Pulser
    {
        /// <summary>
        /// Gets or sets the type of the pulser.
        /// </summary>
        /// <value>The type of the pulser.</value>
        public PulserType PulserType { get; private set; }

        /// <summary>
        /// Gets or sets the frequency.
        /// This is the rate at which this instances pulses, after Delay has passed.
        /// </summary>
        /// <value>The frequency.</value>
        public TimeSpan Frequency { get; private set; }

        /// <summary>
        /// Gets or sets the delay.
        /// This is the time after the pulsor is started or restarted, before it begins pulsing.
        /// </summary>
        /// <value>The delay.</value>
        public TimeSpan Delay { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is signalled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is signalled; otherwise, <c>false</c>.
        /// </value>
        public bool IsSignalled { get; private set; }

        /// <summary>
        /// Occurs when the pulsor is triggered.
        /// </summary>
        public event Action Signalled;

        private DateTime lastPulsed;
        private bool running;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pulser"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="frequency">The frequency.</param>
        public Pulser(PulserType type, TimeSpan frequency)
            : this(type, frequency, TimeSpan.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pulser"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="frequency">The frequency. This is the rate at which this instances pulses, after Delay has passed.</param>
        /// <param name="delay">The delay. This is the time after the pulsor is started or restarted, before it begins pulsing.</param>
        public Pulser(PulserType type, TimeSpan frequency, TimeSpan delay)
            : this(type, frequency, delay, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pulser"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="frequency">The frequency. This is the rate at which this instances pulses, after Delay has passed.</param>
        /// <param name="delay">The delay. This is the time after the pulsor is started or restarted, before it begins pulsing.</param>
        /// <param name="initialState">if set to <c>true</c> IsSignalled will initially be <c>true</c>.</param>
        public Pulser(PulserType type, TimeSpan frequency, TimeSpan delay, bool initialState)
        {
            this.PulserType = type;
            this.Frequency = frequency;
            this.Delay = delay;
            this.IsSignalled = initialState;
            this.running = initialState;
            this.lastPulsed = DateTime.Now;
        }

        /// <summary>
        /// Restarts this pulser.
        /// </summary>
        /// <param name="initialState">if set to <c>true</c> IsSignalled will initially be <c>true</c>.</param>
        /// <param name="resetDelay">if set to <c>true</c> the pulser will wait for the delay again before starting.</param>
        public void Restart(bool initialState, bool resetDelay)
        {
            IsSignalled = initialState;
            lastPulsed = DateTime.Now;
            running = !resetDelay;
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update()
        {
            var now = DateTime.Now;

            if (!running)
            {
                if (now - lastPulsed >= Delay)
                {
                    running = true;
                    Pulse();
                    lastPulsed = now;
                }
                else
                    return;
            }

            switch (PulserType)
            {
                case PulserType.Simple:
                    IsSignalled = false;
                    if (now - lastPulsed >= Frequency)
                        Pulse();
                    while (now - lastPulsed >= Frequency)
                        lastPulsed += Frequency;
                    break;
                case PulserType.Reliable:
                    IsSignalled = false;
                    while (now - lastPulsed >= Frequency)
                    {
                        Pulse();
                        lastPulsed += Frequency;
                    }
                    break;
                case PulserType.SquareWave:
                    while (now - lastPulsed >= Frequency)
                    {
                        IsSignalled = !IsSignalled;
                        if (IsSignalled)
                            OnSignalled();
                        lastPulsed += Frequency;
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnSignalled()
        {
            if (Signalled != null)
                Signalled();
        }

        private void Pulse()
        {
            IsSignalled = true;
            OnSignalled();
        }
    }
}