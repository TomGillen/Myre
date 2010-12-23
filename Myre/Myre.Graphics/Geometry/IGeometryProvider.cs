using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Collections;
using Microsoft.Xna.Framework;
using Myre.Entities;

namespace Myre.Graphics.Geometry
{
    public interface IGeometryProvider
    {
        void Draw(string phase, BoxedValueStore<string> metadata);
        void Query(IList<Entity> results, BoundingVolume volume, bool detailedCheck = false);
        void Query(IList<Entity> results, Ray ray, bool detailedCheck = false);
        void Query(IList<ICullable> results, BoundingVolume volume, bool detailedCheck = false);
        void Query(IList<ICullable> results, Ray ray, bool detailedCheck = false);
    }
}
