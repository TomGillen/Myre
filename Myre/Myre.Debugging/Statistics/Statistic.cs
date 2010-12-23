using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Collections;

namespace Myre.Debugging.Statistics
{
    /// <summary>
    /// A statistic which can be tracked by tools in the Myre.Debugging library.
    /// </summary>
    public class Statistic
        : IDisposableObject
    {
        static Dictionary<string, Statistic> statistics = new Dictionary<string, Statistic>();
        static ReadOnlyDictionary<string, Statistic> readOnlyStatistics = new ReadOnlyDictionary<string, Statistic>(statistics);
        public static ReadOnlyDictionary<string, Statistic> Statistics
        {
            get { return readOnlyStatistics; }
        }


        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public float Value { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value></value>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        /// <value>The format.</value>
        public string Format { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Statistic"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        private Statistic(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the statistic.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Statistic Get(string name, string format = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            Statistic stat;
            if (!statistics.TryGetValue(name, out stat))
            {
                stat = new Statistic(name);
                statistics[name] = stat;
            }

            stat.Format = format ?? stat.Format ?? "{0}";
            return stat;
        }

        public virtual void Dispose()
        {
            statistics.Remove(Name);
            IsDisposed = true;
        }
    }
}
