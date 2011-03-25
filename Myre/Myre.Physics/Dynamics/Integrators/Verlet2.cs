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
    public class Verlet2
        : Integrator<Vector2>
    {
        public Vector2 PreviousPosition
        {
            get;
            private set;
        }

        public Verlet2(string position, string velocity, string acceleration)
            : base(position, velocity, acceleration)
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
            position.Value = (position.Value + velocity.Value) * timeRatio + acceleration.Value * deltaTimeSquare;

            acceleration.Value = Vector2.Zero;
        }

        public void SetPosition(Vector2 pos)
        {
            position.Value = pos;
            PreviousPosition = pos;
        }

        public class Manager
            : Integrator<Vector2>.Manager<Verlet2>
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
