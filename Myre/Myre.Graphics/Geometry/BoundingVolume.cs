﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Geometry
{
    public class BoundingVolume
        : List<Plane>
    {
        private Vector3[] corners = new Vector3[8];

        public BoundingVolume()
        {
        }

        public BoundingVolume(params Plane[] planes)
            : base(planes)
        {
        }

        public void Add(BoundingFrustum frustum)
        {
            Add(Flip(frustum.Near));
            Add(Flip(frustum.Left));
            Add(Flip(frustum.Right));
            Add(Flip(frustum.Bottom));
            Add(Flip(frustum.Far));
            Add(Flip(frustum.Top));
        }

        private Plane Flip(Plane plane)
        {
            return new Plane(-plane.Normal, -plane.D);
        }

        public void Add(BoundingBox box)
        {
            Add(new Plane(Vector3.Up, box.Min.Y));
            Add(new Plane(Vector3.Down, -box.Max.Y));
            Add(new Plane(Vector3.Left, -box.Max.X));
            Add(new Plane(Vector3.Right, box.Min.X));
            Add(new Plane(Vector3.Forward, -box.Max.Z));
            Add(new Plane(Vector3.Backward, box.Min.Z));
        }

        public bool Intersects(Vector3 point)
        {
            for (int i = 0; i < Count; i++)
            {
                var plane = this[i];
                float distance = 0;
                plane.DotCoordinate(ref point, out distance);

                if (distance < 0)
                    return false;
            }

            return true;
        }

        public bool Intersects(BoundingSphere sphere)
        {
            for (int i = 0; i < Count; i++)
            {
                var plane = this[i];
                float distance = 0;
                Vector3.Dot(ref plane.Normal, ref sphere.Center, out distance);

                if (-plane.D - distance > sphere.Radius)
                    return false;
            }

            return true;
        }

        public bool Intersects(BoundingBox box)
        {
            return Intersects(new Vector3(box.Min.X, box.Min.Y, box.Min.Z))
                && Intersects(new Vector3(box.Max.X, box.Min.Y, box.Min.Z))
                && Intersects(new Vector3(box.Min.X, box.Max.Y, box.Min.Z))
                && Intersects(new Vector3(box.Min.X, box.Min.Y, box.Max.Z))
                && Intersects(new Vector3(box.Max.X, box.Max.Y, box.Min.Z))
                && Intersects(new Vector3(box.Min.X, box.Max.Y, box.Max.Z))
                && Intersects(new Vector3(box.Max.X, box.Min.Y, box.Max.Z))
                && Intersects(new Vector3(box.Max.X, box.Max.Y, box.Max.Z));
        }

        public ContainmentType Contains(Vector3 point)
        {
            if (Intersects(point))
                return ContainmentType.Contains;

            return ContainmentType.Disjoint;
        }

        public ContainmentType Contains(BoundingSphere sphere)
        {
            var containment = ContainmentType.Contains;

            for (int i = 0; i < Count; i++)
            {
                var plane = this[i];
                float distance = 0;
                plane.DotCoordinate(ref sphere.Center, out distance);

                if (distance < sphere.Radius)
                {
                    if (distance < -sphere.Radius)
                        return ContainmentType.Disjoint;
                    else
                        containment = ContainmentType.Intersects;
                }
            }

            return containment;
        }

        public ContainmentType Contains(BoundingBox box)
        {
            var outside = 0;

            box.GetCorners(corners);
            for (int i = 0; i < corners.Length; i++)
            {
                if (!Intersects(corners[i]))
                    outside++;

                if (outside > 0 && outside != i)
                    return ContainmentType.Intersects;
            }

            return (outside == corners.Length) ? ContainmentType.Disjoint : ContainmentType.Contains;
        }
    }
}
