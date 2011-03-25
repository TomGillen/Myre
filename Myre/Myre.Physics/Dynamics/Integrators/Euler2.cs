using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Microsoft.Xna.Framework;
using Myre.Entities;
using Ninject;

namespace Myre.Physics.Dynamics.Integrators
{
    [DefaultManager(typeof(Manager))]
    public class Euler2
        : Integrator<Vector2>
    {
        public Euler2(string position, string velocity, string acceleration, string velocityBias)
            : base(position, velocity, acceleration, velocityBias)
        {
        }

        protected override void Integrate(float deltaTime)
        {
            position.Value += (velocity.Value + velocityBias.Value) * deltaTime;
            velocity.Value += acceleration.Value * deltaTime;

            acceleration.Value = Vector2.Zero;
            velocityBias.Value = Vector2.Zero;
        }

        public class Manager
            : Integrator<Vector2>.Manager<Euler2>
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
