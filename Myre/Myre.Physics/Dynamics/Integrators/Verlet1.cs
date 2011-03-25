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
    public class Verlet1
        : Integrator<float>
    {
        public float PreviousPosition
        {
            get;
            private set;
        }

        public Verlet1(string position, string velocity, string acceleration, string velocityBias)
            : base(position, velocity, acceleration, velocityBias)
        {
        }

        public override void Initialise()
        {
            PreviousPosition = position.Value;
        }

        protected override void Integrate(float deltaTimeSquare)
        {
            const float timeRatio = 1;

            velocity.Value = position.Value - PreviousPosition;
            PreviousPosition = position.Value;
            position.Value = (position.Value + velocity.Value + velocityBias.Value) * timeRatio + acceleration.Value * deltaTimeSquare;

            acceleration.Value = 0;
            velocityBias.Value = 0;
        }

        public void SetPosition(float pos)
        {
            position.Value = pos;
            PreviousPosition = pos;
        }

        public class Manager
            : Integrator<float>.Manager<Verlet1>
        {
            public Manager(IKernel kernel)
                : base(kernel, true)
            {

            }

            public override void Update(float elapsedTime)
            {
                float dtSqr = elapsedTime * elapsedTime;

                foreach (var p in Behaviours)
                    p.Integrate(dtSqr);
            }
        }
    }
}
