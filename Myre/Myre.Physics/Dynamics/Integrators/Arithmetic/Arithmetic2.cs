using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Entities;

namespace Myre.Physics.Dynamics.Integrators.Arithmetic
{
    public class Arithmetic2
        :Arithmetic<Vector2>
    {
        public Arithmetic2()
            :base(Vector2.Zero)
        {
        }

        public override Vector2 Add(Vector2 a, Vector2 b)
        {
            var r = a + b;
            return r;
        }

        public override Vector2 Subtract(Property<Vector2> a, Vector2 b)
        {
            var r = a.Value - b;
            return r;
        }

        public override Vector2 Multiply(Vector2 a, float b)
        {
            var r = a * b;
            return r;
        }
    }
}
