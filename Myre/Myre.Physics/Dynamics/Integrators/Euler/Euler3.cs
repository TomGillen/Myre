using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;
using Myre.Physics.Dynamics.Integrators.Arithmetic;

namespace Myre.Physics.Dynamics.Integrators
{
    [DefaultManager(typeof(Manager))]
    public class Euler3
        : Euler<Vector3>
    {
        public Euler3(IntegratorProperties properties)
            : base(properties, new Arithmetic3())
        {
        }
    }
}
