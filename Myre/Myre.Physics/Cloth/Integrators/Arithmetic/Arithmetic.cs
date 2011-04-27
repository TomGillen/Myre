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
        public abstract void Add(ref T a, ref T b, out T result);

        /// <summary>
        /// a - b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="result"></param>
        public abstract void Subtract(ref T a, ref T b, out T result);

        /// <summary>
        /// a * b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public abstract void Multiply(ref T a, float b, out T result);
    }
}
