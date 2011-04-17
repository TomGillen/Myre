using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;
using Myre.Physics.Dynamics.Integrators.Arithmetic;

namespace Myre.Physics.Dynamics.Integrators
{
    [DefaultManager(typeof(Manager))]
    public class Verlet3
        : Verlet<Vector3>
    {
        public Verlet3(IntegratorProperties properties)
            : base(properties, new Arithmetic3())
        {
        }
    }
}
