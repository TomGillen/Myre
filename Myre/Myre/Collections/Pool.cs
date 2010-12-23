using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

namespace Myre.Collections
{
    /// <summary>
    /// Maintains pool of class instances.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object to store. It must define a parameterless constructor, 
    /// and may implement <see cref="IResetable"/>.</typeparam>
    public class Pool<T> where T : class, new()
    {
        static Pool<T> instance;
        /// <summary>
        /// Gets the static instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Pool<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    var p = new Pool<T>();
                    Interlocked.CompareExchange(ref instance, p, null);
                }

                return instance;
            }
        }

        Stack<T> items;
        bool isResetableType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pool&lt;T&gt;"/> class.
        /// </summary>
        public Pool()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pool&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="initialCapacity">The initial number of elements contained within the <see cref="Pool&lt;T&gt;"/>.</param>
        public Pool(int initialCapacity)
        {
            isResetableType = typeof(IRecycleable).IsAssignableFrom(typeof(T));
            items = new Stack<T>(initialCapacity);
            for (int i = 0; i < initialCapacity; i++)
                items.Push(new T());
        }

        /// <summary>
        /// Gets an instance of <typeparamref name="T"/> from the <see cref="Pool&lt;T&gt;"/>
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        public T Get()
        {
            if (items.Count > 0)
            {
                T item = items.Pop();
                if (isResetableType)
                    (item as IRecycleable).Recycle();
                return item;
            }

            return new T();
        }

        /// <summary>
        /// Returns the specified item to the <see cref="Pool&lt;T&gt;"/>.
        /// </summary>
        /// <param name="item">The item to be returned.</param>
        public void Return(T item)
        {
            items.Push(item);
        }
    }
}
