using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities;
using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Ninject;

namespace Myre.Physics.Dynamics.Integrators
{
    public abstract class Integrator<T>
        : Behaviour
    {
        protected Property<T> position;
        protected Property<T> velocity;
        protected Property<T> acceleration;

        private string positionName;
        private string velocityName;
        private string accelerationName;

        protected Integrator(string position, string velocity, string acceleration)
        {
            this.positionName = position;
            this.velocityName = velocity;
            this.accelerationName = acceleration;
        }

        public override void CreateProperties(Entity.InitialisationContext context)
        {
            position = context.GetProperty<T>(positionName);
            velocity = context.GetProperty<T>(velocityName);
            acceleration = context.GetProperty<T>(accelerationName);

            base.CreateProperties(context);
        }

        protected abstract void Integrate(float deltaTime);

        public abstract class Manager<T>
            : BehaviourManager<T>, IProcess
            where T : Behaviour
        {
            public bool IsComplete
            {
                get { return false; }
            }

            public Manager(IKernel kernel, bool requireFixedTimestep)
            {
                var game = kernel.Get<Game>();
                if (requireFixedTimestep && !game.IsFixedTimeStep)
                    throw new NotSupportedException("This integrator does not support non fixed timesteps");

                var scene = kernel.Get<Scene>();
                scene.GetService<ProcessService>().Add(this);
            }

            public abstract void Update(float elapsedTime);
        }
    }
}
