using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.Physics.Collisions
{
    public class Circle
        : Geometry
    {
        private Vector2[] axes;
        private Vector2[] vertices;
        private float radius;
        private Vector2 centre;
        private BoundingBox bounds;

        public override BoundingBox Bounds
        {
            get { return bounds; }
        }

        public float Radius 
        {
            get { return radius; }
            set
            {
                if (radius != value)
                {
                    radius = value;
                    UpdateBounds();
                }
            }
        }

        public Vector2 Centre 
        {
            get { return centre; }
            set
            {
                if (centre != value)
                {
                    centre = value;
                    UpdateBounds();
                }
            }
        }

        public Circle()
        {
            axes = new Vector2[1];
            vertices = new Vector2[2];
        }

        private void UpdateBounds()
        {
            Vector3 c = new Vector3(centre, 0);
            Vector3 extents = new Vector3(radius, radius, 0);
            bounds.Min = c - extents;
            bounds.Max = c + extents;
        }

        public override Projection Project(Vector2 axis)
        {
            Vector2 axisNormalised = Vector2.Normalize(axis);
            Vector2 minIntersection = axisNormalised * -Radius + Centre;
            Vector2 maxIntersection = axisNormalised * Radius + Centre;

            return new Projection(Vector2.Dot(axis, minIntersection), Vector2.Dot(axis, maxIntersection), minIntersection, maxIntersection);
        }

        public override Vector2[] GetAxes(Geometry otherObject)
        {
            axes[0] = otherObject.GetClosestVertex(Centre) - Centre;
            if (axes[0] == Vector2.Zero)
                axes[0] = Vector2.UnitY;

            return axes;
        }

        public override Vector2[] GetVertices(Vector2 axis)
        {
            vertices[0] = Centre + axis * Radius;
            vertices[1] = Centre - axis * Radius;

            return vertices;
        }

        public override Vector2 GetClosestVertex(Vector2 point)
        {
            Vector2 r = point - Centre;
            r *= Radius / r.Length();

            return Centre + r;
        }

        public override bool Contains(Vector2 point)
        {
            Vector2 r = point - Centre;
            return r.LengthSquared() < (Radius * Radius);
        }
    }
}
