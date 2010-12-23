using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Debugging.Statistics
{
    public class StatisticTracker
    {
        Statistic stat;
        DateTime lastAccess;
        TimeSpan accessInterval;
        float lastValue;

        public Statistic Statistic
        {
            get { return stat; }
        }

        public StatisticTracker(Statistic statistic, TimeSpan accessInterval)
        {
            this.stat = statistic;
            this.accessInterval = accessInterval;
            this.lastAccess = DateTime.Now;
            this.lastValue = statistic.Value;
        }

        public float GetValue(out bool read, out bool changed)
        {
            if (stat.IsDisposed)
            {
                read = false;
                changed = false;
                return lastValue;
            }

            changed = false;
            read = false;
            var now = DateTime.Now;
            var dt = now - lastAccess;
            if (dt >= accessInterval)
            {
                changed = lastValue != stat.Value;
                lastValue = stat.Value;
                lastAccess += accessInterval;
                read = true;
            }

            return lastValue;
        }
    }
}
