using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.Physics.Collisions
{
    public struct SatResult
    {
        public Geometry A;
        public Geometry B;
        public Vector2 NormalAxis;
        public float Penetration;
        public Vector2 DeepestPoint;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(SatResult obj)
        {
            return A == obj.A
                && B == obj.B
                && NormalAxis == obj.NormalAxis
                && Penetration == obj.Penetration
                && DeepestPoint == obj.DeepestPoint;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode() ^ NormalAxis.GetHashCode();
        }
    }
}
