using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;

namespace Myre.Physics.Dynamics.Integrators
{
    public class PositionVerlet3
        : Verlet3
    {
        public PositionVerlet3()
            : base(IntegratorProperties.PositionIntegratorProperties)
        {

        }
    }
}
