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
    [DefaultManager(typeof(Manager))]
    public class Euler3
        :Behaviour
    {
        private Property<Vector3> position;
        private Property<Vector3> velocity;
        private Property<Vector3> acceleration;

        private string positionName;
        private string velocityName;
        private string accelerationName;

        public Euler3(string position, string velocity, string acceleration)
        {
            this.positionName = position;
            this.velocityName = velocity;
            this.accelerationName = acceleration;
        }

        public override void CreateProperties(Entity.InitialisationContext context)
        {
            position = context.GetProperty<Vector3>(positionName);
            velocity = context.GetProperty<Vector3>(velocityName);
            acceleration = context.GetProperty<Vector3>(accelerationName);

            base.CreateProperties(context);
        }

        private void Integrate(float deltaTime)
        {
            position.Value += velocity.Value * deltaTime;
            velocity.Value += acceleration.Value * deltaTime;

            acceleration.Value = Vector3.Zero;
        }

        public class Manager
            :BehaviourManager<Euler3>, IProcess
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
