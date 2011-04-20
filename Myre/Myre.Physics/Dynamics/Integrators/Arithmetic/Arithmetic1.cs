using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Entities;

namespace Myre.Physics.Dynamics.Integrators.Arithmetic
{
    public class Arithmetic1
        :Arithmetic<float>
    {
        public static readonly Arithmetic1 Instance = new Arithmetic1();

        public Arithmetic1()
            :base(0)
        {

        }

        public override float Add(float a, float b)
        {
            var r = a + b;
            return r;
        }

        public override float Subtract(Property<float> a, float b)
        {
            var r = a.Value - b;
            return r;
        }

        public override float Multiply(float a, float b)
        {
            var r = a * b;
            return r;
        }
    }
}
