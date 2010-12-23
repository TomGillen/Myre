using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Services;

namespace Myre.Entities.Services
{
    /// <summary>
    /// An interface which defines a service to manage processes.
    /// </summary>
    public interface IProcessService
        : IService
    {
        /// <summary>
        /// Adds the specified process to this instance.
        /// </summary>
        /// <param name="process">The process to add.</param>
        void Add(IProcess process);
    }

    /// <summary>
    /// A class which manages the updating of processes.
    /// </summary>
    public class ProcessService
        : Service, IProcessService
    {
        private List<IProcess> processes;
        private List<IProcess> buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessService"/> class.
        /// </summary>
        public ProcessService()
        {
            processes = new List<IProcess>();
            buffer = new List<IProcess>();
        }

        /// <summary>
        /// Updates the all non-complete processes.
        /// </summary>
        /// <param name="elapsedTime">The number of seconds which have elapsed since the previous frame.</param>
        public override void Update(float elapsedTime)
        {
            var startTime = DateTime.Now;

            lock (buffer)
            {
                processes.AddRange(buffer);
                buffer.Clear();
            }

            for (int i = processes.Count - 1; i >= 0; i--)
            {
                var process = processes[i];

                if (process.IsComplete)
                {
                    processes.RemoveAt(i);
                    continue;
                }

                process.Update(elapsedTime);
            }
        }

        /// <summary>
        /// Adds the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public void Add(IProcess process)
        {
            lock (buffer)
                buffer.Add(process);
        }
    }
}
