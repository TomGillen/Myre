using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.Graphics
{
    interface IBoxIntersectTester
    {
        ContainmentType Intersects(BoundingBox box);
    }

    class BoxBoxIntersectTester
        : IBoxIntersectTester
    {
        public BoundingBox Box { get; set; }
        public ContainmentType Intersects(BoundingBox box)
        {
            return Box.Contains(box);
        }
    }

    class FrustrumBoxIntersectTester
        : IBoxIntersectTester
    {
        public BoundingFrustum Frustrum { get; set; }
        public ContainmentType Intersects(BoundingBox box)
        {
            return Frustrum.Contains(box);
        }
    }

    class SphereBoxIntersectTester
        : IBoxIntersectTester
    {
        public BoundingSphere Sphere { get; set; }
        public ContainmentType Intersects(BoundingBox box)
        {
            return Sphere.Contains(box);
        }
    }
}
