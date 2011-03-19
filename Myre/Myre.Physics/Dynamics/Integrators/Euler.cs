using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Myre.Entities;
using Microsoft.Xna.Framework;

namespace Myre.Physics.Dynamics.Integrators
{
    public class Euler
        :Behaviour
    {
        private Property<Vector3> position;
        private Property<Vector3> velocity;
        private Property<Vector3> acceleration;

        public override void CreateProperties(Entity.InitialisationContext context)
        {
            position = context.GetProperty<Vector3>("position");
            velocity = context.GetProperty<Vector3>("velocity");
            acceleration = context.GetProperty<Vector3>("acceleration");

            base.CreateProperties(context);
        }

        private void Integrate(float deltaTime)
        {
            position.Value += velocity.Value * deltaTime;
            velocity.Value += acceleration.Value * deltaTime;

            acceleration.Value = Vector3.Zero;
        }

        public class Manager
            :BehaviourManager<Euler>, IProcess
        {
            public bool IsComplete
            {
                get { return false; }
            }

            public Manager(Scene scene)
            {
                scene.GetService<ProcessService>().Add(this);
            }

            public void Update(float elapsedTime)
            {
                foreach (var p in Behaviours)
                    p.Integrate(elapsedTime);
            }
        }
    }
}
