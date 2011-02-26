using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Microsoft.Xna.Framework;
using Myre.Entities;

namespace Myre.Physics.Dynamics.Integrators
{
    [DefaultManager(typeof(Manager))]
    public class Verlet
        :Behaviour
    {
        public Vector3 PreviousPosition
        {
            get;
            private set;
        }

        private Property<Vector3> position;
        private Property<Vector3> velocity;
        private Property<Vector3> acceleration;

        public override void CreateProperties(Entity.InitialisationContext context)
        {
            position = context.CreateProperty<Vector3>("position");
            velocity = context.CreateProperty<Vector3>("velocity");
            acceleration = context.CreateProperty<Vector3>("acceleration");

            base.CreateProperties(context);
        }

        public override void Initialise()
        {
            PreviousPosition = position.Value;
        }

        private void Integrate(float deltaTimeSquare)
        {
            float timeRatio = 1;

            velocity.Value = position.Value - PreviousPosition;
            PreviousPosition = position.Value;
            position.Value = (position.Value + velocity.Value) * timeRatio + acceleration.Value * deltaTimeSquare;

            acceleration.Value = Vector3.Zero;
        }

        public class Manager
            :BehaviourManager<Verlet>, IProcess
        {
            public bool IsComplete
            {
                get { return false; }
            }

            public Manager(Scene scene)
            {
                scene.GetService<ProcessService>().Add(this);
            }

#if DEBUG
            bool firstStep = true;
            float previousDt;
#endif

            public void Update(float elapsedTime)
            {
#if DEBUG
                if (firstStep)
                    previousDt = elapsedTime;
                else if (previousDt != elapsedTime)
                    throw new ArgumentException("Verlet integrator requires a fixed timestep");
#endif

                float dtSqr = elapsedTime * elapsedTime;

                foreach (var p in Behaviours)
                    p.Integrate(dtSqr);
            }
        }

        public void Setup(Vector3 pos)
        {
            position.Value = pos;
            PreviousPosition = pos;
        }
    }
}
