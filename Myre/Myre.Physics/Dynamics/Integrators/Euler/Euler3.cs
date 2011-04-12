using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;
using Myre.Physics.Dynamics.Integrators.Arithmetic;

namespace Myre.Physics.Dynamics.Integrators
{
    [DefaultManager(typeof(Manager))]
    public class Euler3
        : Euler<Vector3>
    {
        public Euler3(string position, string velocity, string acceleration, string velocityBias)
            : base(position, velocity, acceleration, velocityBias, new Arithmetic3())
        {
        }
    }
}
