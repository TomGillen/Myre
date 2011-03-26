using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Physics.Dynamics.Integrators
{
    public class PositionVerlet3
        :Verlet3
    {
        public PositionVerlet3()
            :base(PropertyName.POSITION, PropertyName.LINEAR_VELOCITY, PropertyName.ACCELERATION, PropertyName.LINEAR_VELOCITY)
        {

        }
    }
}
