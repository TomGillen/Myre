using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Debugging.Statistics
{
    /// <summary>
    /// Counts the number of times an events happens in a second.
    /// </summary>
    public class FrequencyTracker
    {
        DateTime lastUpdate;
        Statistic statistic;
        int counter;

        /// <summary>
        /// Gets or sets the frequency.
        /// </summary>
        /// <value>The frequency.</value>
        public float Frequency { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyTracker"/> class.
        /// </summary>
        /// <param name="statisticName">Name of the statistic.</param>
        public FrequencyTracker(string statisticName)
        {
            lastUpdate = DateTime.Now;
            if (!string.IsNullOrEmpty(statisticName))
                statistic = Statistic.Get(statisticName);
        }

        /// <summary>
        /// Pulses this instance.
        /// </summary>
        public void Pulse()
        {
            counter++;
            var now = DateTime.Now;
            if ((now - lastUpdate).TotalSeconds > 1f)
            {
                Frequency = counter;
                if (statistic != null)
                    statistic.Value = counter;
                counter = 0;
                lastUpdate = now;
            }
        }
    }
}
