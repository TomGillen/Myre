using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Entities;

namespace Myre.Physics.Dynamics.Integrators.Arithmetic
{
    public class Arithmetic3
        :Arithmetic<Vector3>
    {
        public Arithmetic3()
            :base(Vector3.Zero)
        {
        }

        public override Vector3 Add(Vector3 a, Vector3 b)
        {
            var r = a + b;
            return r;
        }

        public override Vector3 Subtract(Property<Vector3> a, Vector3 b)
        {
            var r = a.Value - b;
            return r;
        }

        public override Vector3 Multiply(Vector3 a, float b)
        {
            var r = a * b;
            return r;
        }
    }
}
