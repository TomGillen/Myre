using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Entities;
using Ninject;

namespace Myre.Physics.Dynamics.Integrators
{
    [DefaultManager(typeof(Manager))]
    public class Euler1
        : Integrator<float>
    {
        public Euler1(string position, string velocity, string acceleration, string velocityBias)
            : base(position, velocity, acceleration, velocityBias)
        {
        }

        protected override void Integrate(float deltaTime)
        {
            position.Value += (velocity.Value + velocityBias.Value) * deltaTime;
            velocity.Value += acceleration.Value * deltaTime;

            acceleration.Value = 0;
            velocityBias.Value = 0;
        }

        public class Manager
            : Integrator<float>.Manager<Euler1>
        {
            public Manager(IKernel kernel)
                : base(kernel, false)
            {
            }

            public override void Update(float elapsedTime)
            {
                foreach (var p in Behaviours)
                    p.Integrate(elapsedTime);
            }
        }
    }
}
