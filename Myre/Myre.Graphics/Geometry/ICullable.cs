using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.Graphics
{
    public interface ICullable
    {
        BoundingSphere Bounds { get; }
        //OrientedBoundingBox BoundingBox { get; }
        //bool IsVisble { get;  set; }
    }
}
