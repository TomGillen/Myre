using Myre.Entities;
using Myre.Entities.Services;
using Myre.Physics.Dynamics.Integrators.Arithmetic;
using Myre.Entities.Behaviours;

namespace Myre.Physics.Dynamics.Integrators
{
    public abstract class Verlet<T>
        :Integrator<T>
    {
        T previousPosition;
        public T PreviousPosition
        {
            get
            {
                return previousPosition;
            }
            protected set
            {
                previousPosition = value;
            }
        }

        public Verlet(IntegratorProperties properties, Arithmetic<T> arithmetic)
            : base(properties, arithmetic)
        {
        }

        protected void Integrate(float dT, float deltaTimeSquare)
        {
            //const float timeRatio = 1;

            //velocity.Value = position.Value - previousPosition;
            velocity.Value = Arithmetic.Subtract(position, previousPosition);

            previousPosition = position.Value;
            
            //position.Value = (position.Value + velocity.Value + velocityBias.Value) * timeRatio + acceleration.Value * deltaTimeSquare;
            var sumPositionVelocityBias = Arithmetic.Add(Arithmetic.Add(position.Value, velocity.Value), (velocityBias == null ? Arithmetic.Zero : velocityBias.Value));
            var accelerationTerm = Arithmetic.Multiply(acceleration.Value, deltaTimeSquare);
            position.Value = Arithmetic.Add(sumPositionVelocityBias, accelerationTerm);

            velocity.Value = Arithmetic.Multiply(velocity.Value, 1 / dT);

            acceleration.Value = Arithmetic.Zero;
            if (velocityBias != null)
                velocityBias.Value = Arithmetic.Zero;
        }

        public override void SetPosition(T pos)
        {
            PreviousPosition = pos;
            base.SetPosition(pos);
        }

        public class Manager
            : Integrator<T>.Manager<Verlet<T>>
        {
            private float dtSquare;

            public Manager(Scene scene, NinjectGame game)
                :base(scene.GetService<ProcessService>(), game, true)
            {

            }

            protected override void PrepareUpdate(float elapsedTime)
            {
                dtSquare = elapsedTime * elapsedTime;
            }

            protected override void Update(Verlet<T> behaviour, float elapsedTime)
            {
                behaviour.Integrate(elapsedTime, dtSquare);
            }
        }
    }
}
