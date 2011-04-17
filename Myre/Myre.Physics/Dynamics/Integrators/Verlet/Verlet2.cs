using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;
using Myre.Physics.Dynamics.Integrators.Arithmetic;

namespace Myre.Physics.Dynamics.Integrators
{
    [DefaultManager(typeof(Manager))]
    public class Verlet2
        : Verlet<Vector2>
    {
        public Verlet2(IntegratorProperties properties)
            : base(properties, new Arithmetic2())
        {
        }
    }
}
