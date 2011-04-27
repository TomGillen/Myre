using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Entities;

namespace Myre.Physics.Dynamics.Integrators.Arithmetic
{
    public sealed class Arithmetic3
        : Arithmetic<Vector3>
    {
        public static readonly Arithmetic3 Instance = new Arithmetic3();

        public Arithmetic3()
            :base(Vector3.Zero)
        {
        }

        public override void Add(ref Vector3 a, ref Vector3 b, out Vector3 result)
        {
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            result.Z = a.Z + b.Z;
        }

        public override void Subtract(ref Vector3 a, ref Vector3 b, out Vector3 result)
        {
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
            result.Z = a.Z - b.Z;
        }

        public override void Multiply(ref Vector3 a, float b, out Vector3 result)
        {
            result.X = a.X * b;
            result.Y = a.Y * b;
            result.Z = a.Z * b;
        }
    }
}
