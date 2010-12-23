using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Physics.Collisions
{
    class Pool<T>
        where T : class, new()
    {
        Stack<T> items;

        public Pool()
        {
            items = new Stack<T>();
        }

        public void Return(T item)
        {
            items.Push(item);
        }

        public T Get()
        {
            if (items.Count > 0)
                return items.Pop();
            else
                return new T();
        }
    }
}
