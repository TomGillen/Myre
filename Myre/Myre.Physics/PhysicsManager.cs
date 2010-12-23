using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Physics
{
    public class PhysicsManager
    {
        internal float inverseDT;

        public float AllowedPenetration { get; set; }
        public float BiasFactor { get; set; }
    }
}
