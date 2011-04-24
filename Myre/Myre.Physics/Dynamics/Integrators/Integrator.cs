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

        private readonly IntegratorProperties propertyNames;

        protected readonly Arithmetic<T> Arithmetic;

        protected Integrator(string position, string velocity, string acceleration, string velocityBias, Arithmetic<T> arithmetic)
            :this(new IntegratorProperties(position, velocity, acceleration, velocityBias), arithmetic)
        {
        }

        protected Integrator(IntegratorProperties names, Arithmetic<T> arithmetic)
        {
            propertyNames = names;
            this.Arithmetic = arithmetic;
        }

        public override void CreateProperties(Entity.InitialisationContext context)
        {
            position = context.CreateProperty<T>(propertyNames.PositionName);
            velocity = context.CreateProperty<T>(propertyNames.VelocityName);
            acceleration = context.CreateProperty<T>(propertyNames.AccelerationName);

            if (propertyNames.VelocityBiasName != null)
                velocityBias = context.GetProperty<T>(propertyNames.VelocityBiasName);

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

    public struct IntegratorProperties
    {
        public static readonly IntegratorProperties POSITION = new IntegratorProperties(PhysicsProperties.POSITION, PhysicsProperties.LINEAR_VELOCITY, PhysicsProperties.LINEAR_ACCELERATION, PhysicsProperties.LINEAR_VELOCITY_BIAS);
        public static readonly IntegratorProperties ROTATION = new IntegratorProperties(PhysicsProperties.ROTATION, PhysicsProperties.ANGULAR_VELOCITY, PhysicsProperties.ANGULAR_ACCELERATION, PhysicsProperties.ANGULAR_VELOCITY_BIAS);

        public readonly string PositionName;
        public readonly string VelocityName;
        public readonly string AccelerationName;
        public readonly string VelocityBiasName;

        public IntegratorProperties(string position, string velocity, string acceleration, string velocityBias)
        {
            PositionName = position;
            VelocityName = velocity;
            AccelerationName = acceleration;
            VelocityBiasName = velocityBias;
        }
    }
}
