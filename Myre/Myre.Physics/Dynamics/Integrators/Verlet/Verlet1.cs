using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;
using Myre.Physics.Dynamics.Integrators.Arithmetic;

namespace Myre.Physics.Dynamics.Integrators
{
    [DefaultManager(typeof(Manager))]
    public class Verlet1
        : Verlet<float>
    {
        public Verlet1(IntegratorProperties properties)
            : base(properties, new Arithmetic1())
        {
        }
    }
}
