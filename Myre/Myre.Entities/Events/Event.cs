using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Entities.Events
{
    /// <summary>
    /// A class which represents an event for a specified data type.
    /// Instances of this type can be used to send events to listeners which have registered with this event.
    /// </summary>
    /// <typeparam name="Data">The type of payload data this event sends.</typeparam>
    public class Event<Data>
    {
        class Invocation
            : IEventInvocation
        {
            static Queue<Invocation> pool = new Queue<Invocation>();
            static SpinLock spinLock;

            public Data Data;
            public Event<Data> Event;

            public void Execute()
            {
                for (int i = 0; i < Event.listeners.Count; i++)
                    Event.listeners[i].HandleEvent(Data);
            }

            public void Recycle()
            {
                Data = default(Data);
                try
                {
                    spinLock.Lock();
                    pool.Enqueue(this);
                }
                finally
                {
                    spinLock.Unlock();
                }
            }

            public static Invocation Get()
            {
                try
                {
                    spinLock.Lock();
                    if (pool.Count > 0)
                        return pool.Dequeue();
                    else
                        return new Invocation();
                }
                finally
                {
                    spinLock.Unlock();
                }
            }
        }

        private EventService service;
        private List<IEventListener<Data>> listeners;

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <value>The service.</value>
        public EventService Service { get { return service; } }

        internal Event(EventService service)
        {
            this.service = service;
            this.listeners = new List<IEventListener<Data>>();
        }

        /// <summary>
        /// Adds a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public void AddListener(IEventListener<Data> listener)
        {
            listeners.Add(listener);
        }

        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <returns></returns>
        public bool RemoveListener(IEventListener<Data> listener)
        {
            return listeners.Remove(listener);
        }

        /// <summary>
        /// Sends the specified data to all registered listeners.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send(Data data)
        {
            Invocation invocation = Invocation.Get();
            invocation.Event = this;
            invocation.Data = data;

            service.Queue(invocation);
        }
    }
}
