using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities;

namespace Myre.Physics.Dynamics.Integrators.Arithmetic
{
    public abstract class Arithmetic<T>
    {
        public readonly T Zero;

        public Arithmetic(T zero)
        {
            Zero = zero;
        }

        /// <summary>
        /// a + b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public abstract T Add(T a, T b);

        /// <summary>
        /// a - b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="result"></param>
        public abstract T Subtract(Property<T> a, T b);

        /// <summary>
        /// a * b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public abstract T Multiply(T a, float b);
    }
}
