using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.Physics.Collisions
{
    public class Polygon
        : Geometry
    {
        private Vector2[] localVertices;
        private Vector2[] worldVertices;
        private Vector2[] localAxes;
        private Vector2[] worldAxes;
        private Matrix transform;
        private bool transformDirty;
        private Vector2[] localBounds;
        private Vector2[] worldBounds;
        private BoundingBox bounds;

        public Matrix Transform
        {
            get { return transform; }
            set
            {
                if (transform != value)
                {
                    transform = value;
                    transformDirty = true;
                    RecalculateBounds();
                }
            }
        }

        public Vector2[] WorldVertices
        {
            get 
            {
                if (transformDirty)
                    ApplyTransform();
                return worldVertices; 
            }
        }

        public Vector2[] LocalVertices
        {
            get 
            {
                if (transformDirty)
                    ApplyTransform();
                return localVertices; 
            }
        }

        public override BoundingBox Bounds
        {
            get { return bounds; }
        }

        public Polygon(Vector2[] vertices)
        {
            this.localVertices = vertices;
            this.worldVertices = new Vector2[localVertices.Length];

            CalculateAxes();
            SetupBounds();
            Transform = Matrix.Identity;
        }

        private void SetupBounds()
        {
            bounds.Min = new Vector3(float.MaxValue, float.MaxValue, 0);
            bounds.Max = new Vector3(float.MinValue, float.MinValue, 0);
            for (int i = 0; i < localVertices.Length; i++)
            {
                var v = localVertices[i];
                if (bounds.Min.X > v.X)
                    bounds.Min.X = v.X;
                if (bounds.Min.Y > v.Y)
                    bounds.Min.Y = v.Y;
                if (bounds.Max.X < v.X)
                    bounds.Max.X = v.X;
                if (bounds.Max.Y < v.Y)
                    bounds.Max.Y = v.Y;
            }

            localBounds = new Vector2[4];
            worldBounds = new Vector2[4];

            localBounds[0] = worldBounds[0] = new Vector2(bounds.Min.X, bounds.Min.Y);
            localBounds[1] = worldBounds[1] = new Vector2(bounds.Max.X, bounds.Min.Y);
            localBounds[2] = worldBounds[2] = new Vector2(bounds.Max.X, bounds.Max.Y);
            localBounds[3] = worldBounds[3] = new Vector2(bounds.Min.X, bounds.Max.Y);
        }

        private void RecalculateBounds()
        {
            Vector2.Transform(localBounds, ref transform, worldBounds);
            bounds.Min = new Vector3(float.MaxValue, float.MaxValue, 0);
            bounds.Max = new Vector3(float.MinValue, float.MinValue, 0);
            for (int i = 0; i < worldBounds.Length; i++)
            {
                var v = worldBounds[i];
                if (bounds.Min.X > v.X)
                    bounds.Min.X = v.X;
                if (bounds.Min.Y > v.Y)
                    bounds.Min.Y = v.Y;
                if (bounds.Max.X < v.X)
                    bounds.Max.X = v.X;
                if (bounds.Max.Y < v.Y)
                    bounds.Max.Y = v.Y;
            }
        }

        private void ApplyTransform()
        {
            Vector2.Transform(localVertices, ref transform, worldVertices);
            Vector2.TransformNormal(localAxes, ref transform, worldAxes);

            transformDirty = false;
        }

        private void CalculateAxes()
        {
            worldAxes = new Vector2[localVertices.Length];
            localAxes = new Vector2[localVertices.Length];

            for (int i = 0; i < worldAxes.Length; i++)
            {
                var start = localVertices[i];
                var end = localVertices[(i + 1) % localVertices.Length];

                var r = end - start;
                worldAxes[i] = localAxes[i] = new Vector2(-r.Y, r.X);
            }
        }

        public override Vector2[] GetAxes(Geometry otherObject)
        {
            if (transformDirty)
                ApplyTransform();

            return worldAxes;
        }

        public override Vector2[] GetVertices(Vector2 axis)
        {
            if (transformDirty)
                ApplyTransform();

            return worldVertices;
        }

        public override Vector2 GetClosestVertex(Vector2 point)
        {
            if (transformDirty)
                ApplyTransform();

            // TODO: more directed search for closest vertex on a polygon
            Vector2 closest = worldVertices[0];
            float distance = (closest - point).LengthSquared();
            for (int i = 1; i < worldVertices.Length; i++)
            {
                var v = worldVertices[i];
                var r = v - point;
                var d = r.LengthSquared();

                if (d < distance)
                {
                    distance = d;
                    closest = v;
                }
            }

            return closest;
        }

        public override Projection Project(Vector2 axis)
        {
            if (transformDirty)
                ApplyTransform();
            return Projection.Create(axis, worldVertices);
        }

        // algorithm found here:
        // http://local.wasp.uwa.edu.au/~pbourke/geometry/insidepoly/
        public override bool Contains(Vector2 p)
        {
            if (transformDirty)
                ApplyTransform();

            int counter = 0;
            int i;
            double xinters;
            Vector2 p1, p2;

            p1 = worldVertices[0];
            for (i = 1; i <= worldVertices.Length; i++)
            {
                p2 = worldVertices[i % worldVertices.Length];
                if (p.Y > Math.Min(p1.Y, p2.Y))
                {
                    if (p.Y <= Math.Max(p1.Y, p2.Y))
                    {
                        if (p.X <= Math.Max(p1.X, p2.X))
                        {
                            if (p1.Y != p2.Y)
                            {
                                xinters = (p.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                                if (p1.X == p2.X || p.X <= xinters)
                                    counter++;
                            }
                        }
                    }
                }
                p1 = p2;
            }

            return (counter % 2 != 0);
        }
    }
}
