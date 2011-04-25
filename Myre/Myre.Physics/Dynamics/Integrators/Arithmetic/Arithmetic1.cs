using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Entities;

namespace Myre.Physics.Dynamics.Integrators.Arithmetic
{
    public sealed class Arithmetic1
        : Arithmetic<float>
    {
        public static readonly Arithmetic1 Instance = new Arithmetic1();

        public Arithmetic1()
            : base(0)
        {

        }

        public override void Add(ref float a, ref float b, out float result)
        {
            result = a + b;
        }

        public override void Subtract(ref float a, ref float b, out float result)
        {
            result = a - b;
        }

        public override void Multiply(ref float a, float b, out float result)
        {
            result = a * b;
        }
    }
}
