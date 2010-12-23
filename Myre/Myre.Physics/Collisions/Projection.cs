﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.Physics.Collisions
{
    public struct Projection
    {
        public float Start;
        public float End;
        public Vector2 StartPoint;
        public Vector2 EndPoint;

        public Projection(float a, float b)
        {
            if (a < b)
            {
                Start = a;
                End = b;
            }
            else
            {
                Start = b;
                End = a;
            }

            StartPoint = EndPoint = Vector2.Zero;
        }

        public Projection(float a, float b, Vector2 pointA, Vector2 pointB)
        {
            if (a < b)
            {
                Start = a;
                StartPoint = pointA;

                End = b;
                EndPoint = pointB;
            }
            else
            {
                Start = b;
                StartPoint = pointB;

                End = a;
                EndPoint = pointA;
            }
        }

        public bool Overlaps(Projection b)
        {
            if (this.Start > b.End)
                return false;

            if (this.End < b.Start)
                return false;

            return true;
        }

        public bool Overlaps(Projection b, out float distance)
        {
            int order;
            return Overlaps(b, out distance, out order);
        }

        public bool Overlaps(Projection b, out float distance, out int order)
        {
            var startDistance = Math.Abs(this.Start - b.End);
            var endDistance = Math.Abs(this.End - b.Start);

            if (startDistance < endDistance)
            {
                distance = startDistance;
                order = -1;
            }
            else
            {
                distance = endDistance;
                order = 1;
            }

            if (this.Start > b.End)
                return false;

            if (this.End < b.Start)
                return false;

            return true;
        }

        public static Projection Create(Vector2 axis, Vector2[] vertices)
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            Vector2 minPoint = vertices[0];
            Vector2 maxPoint = vertices[0];

            for (int i = 0; i < vertices.Length; i++)
            {
                float projection = Vector2.Dot(axis, vertices[i]);

                if (min > projection)
                {
                    min = projection;
                    minPoint = vertices[i];
                }

                if (max < projection)
                {
                    max = projection;
                    maxPoint = vertices[i];
                }
            }

            return new Projection() { Start = min, End = max, StartPoint = minPoint, EndPoint = maxPoint };
        }

        public override bool Equals(object obj)
        {
            if (obj is Projection)
                return Equals((Projection)obj);
            else
                return base.Equals(obj);
        }

        public bool Equals(Projection other)
        {
            return this.Start == other.Start && this.End == other.End;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Start.GetHashCode() ^ End.GetHashCode();
        }
    }
}
