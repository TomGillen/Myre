using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;
using Myre.Physics.Dynamics.Integrators.Arithmetic;

namespace Myre.Physics.Dynamics.Integrators
{
    [DefaultManager(typeof(Manager))]
    public class Euler2
        : Euler<Vector2>
    {
        public Euler2(IntegratorProperties properties)
            : base(properties, new Arithmetic2())
        {
        }
    }
}
