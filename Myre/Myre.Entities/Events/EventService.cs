using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Services;
using Myre;

namespace Myre.Entities.Events
{
    interface IEventInvocation
        : IRecycleable
    {
        void Execute();
    }

    /// <summary>
    /// An interface which defines methods for retrieving events.
    /// </summary>
    public interface IEventService
        : IService
    {
        Event<T> GetEvent<T>();
    }

    /// <summary>
    /// A class which manages the sending of events.
    /// </summary>
    public class EventService
        : Service, IEventService
    {
        private Dictionary<Type, object> events;
        private Queue<IEventInvocation> waitingEvents;
        private Queue<IEventInvocation> executingEvents;
        private SpinLock spinLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventService"/> class.
        /// </summary>
        public EventService()
        {
            events = new Dictionary<Type, object>();
            waitingEvents = new Queue<IEventInvocation>();
            executingEvents = new Queue<IEventInvocation>();
        }

        /// <summary>
        /// Gets an event of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of data this event sends.</typeparam>
        /// <returns></returns>
        public Event<T> GetEvent<T>()
        {
            var type = typeof(T);
            object e = null;

            if (!events.TryGetValue(type, out e))
            {
                e = new Event<T>(this);
                events[type] = e;
            }

            return e as Event<T>;
        }

        /// <summary>
        /// Sends any queued events.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        public override void Update(float elapsedTime)
        {
            while (waitingEvents.Count > 0)
            {
                FlipBuffers();
                ExecuteEvents();
            }
        }

        internal void Queue(IEventInvocation eventInvocation)
        {
            try
            {
                spinLock.Lock();
                waitingEvents.Enqueue(eventInvocation);
            }
            finally
            {
                spinLock.Unlock();
            }
        }

        private void ExecuteEvents()
        {
            while (executingEvents.Count > 0)
            {
                IEventInvocation invocation = executingEvents.Dequeue();
                invocation.Execute();
                invocation.Recycle();
            }
        }

        private void FlipBuffers()
        {
            try
            {
                spinLock.Lock();
                var tmp = waitingEvents;
                waitingEvents = executingEvents;
                executingEvents = tmp;
            }
            finally
            {
                spinLock.Unlock();
            }
        }
    }
}
