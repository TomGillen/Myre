using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre;
using Myre.Extensions;

namespace Myre.Graphics
{
    public class OrientedBoundingBox
    {
        private BoundingBox bounds;
        private Matrix transform;
        private BoundingBox axisAligned;
        private bool dirty;

        public BoundingBox LocalBounds
        {
            get { return bounds; }
            set
            {
                if (bounds != value)
                {
                    bounds = value;
                    dirty = true;
                }
            }
        }

        public Matrix Transform
        {
            get { return transform; }
            set
            {
                if (transform != value)
                {
                    transform = value;
                    dirty = true;
                }
            }
        }

        public BoundingBox AxisAlignedBounds
        {
            get
            {
                if (dirty) Update();
                return axisAligned;
            }
        }

        private void Update()
        {
            axisAligned = bounds.Transform(ref transform);
            dirty = false;
        }
    }
}
