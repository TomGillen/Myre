using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;

namespace Myre.Physics.Collisions
{
    /// <summary>
    /// Represents a piece of geometry which can be used for collision detection.
    /// </summary>
    public abstract class Geometry
    {
        private float restitution;
        internal List<Geometry> collidingWith;

        public DynamicPhysics Body { get; set; }

        /// <summary>
        /// Gets or sets the restitution coefficient.
        /// This determines the 'bounciness' of the object.
        /// A value of 0 will act like a lump of clay, hitting objects and stickig to them; a value of 1 will act like a billiard ball.
        /// </summary>
        public float Restitution
        {
            get { return restitution; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("restitution must be between 0 and 1.");
                restitution = value;
            }
        }

        /// <summary>
        /// Gets or sets the friction coefficient.
        /// Higher values indicate a rougher surface.
        /// </summary>
        public float FrictionCoefficient { get; set; }

        /// <summary>
        /// Gets an axis aligned bounding box for this geometry.
        /// </summary>
        public abstract BoundingBox Bounds { get; }

        /// <summary>
        /// Gets a collection containing all geometry this geometry is colliding with.
        /// </summary>
        public ReadOnlyCollection<Geometry> CollidingWith { get; private set; }

        public Geometry()
        {
            collidingWith = new List<Geometry>();
            CollidingWith = new ReadOnlyCollection<Geometry>(collidingWith);
        }

        /// <summary>
        /// Gets an array of axes, each pointing out from each face on this geometry.
        /// </summary>
        /// <param name="otherObject">The other geometry instance this instance is to be tested against.</param>
        /// <returns>An array of axes, each pointing out from each face on this geometry.</returns>
        public abstract Vector2[] GetAxes(Geometry otherObject);

        /// <summary>
        /// Gets the vertices which form this geometry.
        /// </summary>
        /// <param name="axis">The axis this geometry will be projected onto.</param>
        /// <returns>The vertices which form this geometry.</returns>
        public abstract Vector2[] GetVertices(Vector2 axis);

        /// <summary>
        /// Gets the vertex on this geometry which is closest to the specified point.
        /// </summary>
        /// <param name="point">The point to be tested.</param>
        /// <returns>The vertex on this geometry which is closest to the specified point.</returns>
        public abstract Vector2 GetClosestVertex(Vector2 point);

        /// <summary>
        /// Projects this geometry onto the specified axis
        /// </summary>
        /// <param name="axis">The axis to be projected onto.</param>
        /// <returns>The projection of this geometry onto the specified axis.</returns>
        public abstract Projection Project(Vector2 axis);

        /// <summary>
        /// Determines if this geometry contains the specified point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public abstract bool Contains(Vector2 point);
    }
}