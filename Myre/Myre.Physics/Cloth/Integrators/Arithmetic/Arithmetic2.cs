using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Entities;

namespace Myre.Physics.Dynamics.Integrators.Arithmetic
{
    public sealed class Arithmetic2
        : Arithmetic<Vector2>
    {
        public static readonly Arithmetic2 Instance = new Arithmetic2();

        public Arithmetic2()
            : base(Vector2.Zero)
        {
        }

        public override void Add(ref Vector2 a, ref Vector2 b, out Vector2 result)
        {
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
        }

        public override void Subtract(ref Vector2 a, ref Vector2 b, out Vector2 result)
        {
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
        }

        public override void Multiply(ref Vector2 a, float b, out Vector2 result)
        {
            result.X = a.X * b;
            result.Y = a.Y * b;
        }
    }
}
