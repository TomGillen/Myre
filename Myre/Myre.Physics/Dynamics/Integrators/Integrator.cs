using System;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Myre.Physics.Dynamics.Integrators.Arithmetic;

namespace Myre.Physics.Dynamics.Integrators
{
    public abstract class Integrator<T>
        : Behaviour
    {
        protected Property<T> position;
        protected Property<T> velocity;
        protected Property<T> acceleration;
        protected Property<T> velocityBias;

        private readonly string positionName;
        private readonly string velocityName;
        private readonly string accelerationName;
        private readonly string velocityBiasName;

        protected readonly Arithmetic<T> Arithmetic;

        protected Integrator(string position, string velocity, string acceleration, string velocityBias, Arithmetic<T> arithmetic)
        {
            this.positionName = position;
            this.velocityName = velocity;
            this.accelerationName = acceleration;
            this.velocityBiasName = velocityBias;
            this.Arithmetic = arithmetic;
        }

        public override void CreateProperties(Entity.InitialisationContext context)
        {
            position = context.CreateProperty<T>(positionName);
            velocity = context.CreateProperty<T>(velocityName);
            acceleration = context.CreateProperty<T>(accelerationName);
            velocityBias = context.GetProperty<T>(velocityBiasName);

            base.CreateProperties(context);
        }

        public virtual void SetPosition(T pos)
        {
            position.Value = pos;
        }

        public override void Initialise()
        {
            SetPosition(position.Value);
        }

        public abstract class Manager<B>
            : BehaviourManager<B>, IProcess
            where B : Behaviour
        {
            public bool IsComplete
            {
                get { return false; }
            }

            public Manager([SceneService]ProcessService process, NinjectGame game, bool requireFixedTimestep)
            {
                if (requireFixedTimestep && !game.IsFixedTimeStep)
                    throw new InvalidOperationException("Integrator requires a fixed time step");

                process.Add(this);
            }

            protected abstract void PrepareUpdate(float elapsedTime);

            protected abstract void Update(B behaviour, float elapsedTime);

            public void Update(float elapsedTime)
            {
                PrepareUpdate(elapsedTime);

                foreach (var item in Behaviours)
                    Update(item, elapsedTime);
            }
        }
    }
}
