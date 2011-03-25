using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Myre.Entities;
using Microsoft.Xna.Framework;
using Ninject;

namespace Myre.Physics.Dynamics.Integrators
{
    [DefaultManager(typeof(Manager))]
    public class Euler3
        :Integrator<Vector3>
    {
        public Euler3(string position, string velocity, string acceleration, string velocityBias)
            :base(position, velocity, acceleration, velocityBias)
        {
        }

        protected override void Integrate(float deltaTime)
        {
            position.Value += (velocity.Value + velocityBias.Value) * deltaTime;
            velocity.Value += acceleration.Value * deltaTime;

            acceleration.Value = Vector3.Zero;
        }

        public class Manager
            :Integrator<Vector3>.Manager<Euler3>
        {
            public Manager(IKernel kernel)
                :base(kernel, false)
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
