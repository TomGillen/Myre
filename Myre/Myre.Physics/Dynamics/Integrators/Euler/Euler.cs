using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Physics.Dynamics.Integrators.Arithmetic;
using Myre.Entities;
using Myre.Entities.Services;
using Myre.Entities.Behaviours;

namespace Myre.Physics.Dynamics.Integrators
{
    public abstract class Euler<T>
        :Integrator<T>
    {
        public Euler(string position, string velocity, string acceleration, string velocityBias, Arithmetic<T> arithmetic)
            :base(position, velocity, acceleration, velocityBias, arithmetic)
        {

        }

        public Euler(IntegratorProperties properties, Arithmetic<T> arithmetic)
            : base(properties, arithmetic)
        {

        }

        public void Integrate(float deltaTime)
        {
            velocity.Value = Arithmetic.Add(velocity.Value, Arithmetic.Multiply(acceleration.Value, deltaTime));

            var v = velocityBias == null ? velocity.Value : Arithmetic.Add(velocity.Value, velocityBias.Value);
            var deltaPosition = Arithmetic.Multiply(v, deltaTime);

            position.Value = Arithmetic.Add(position.Value, deltaPosition);

            acceleration.Value = Arithmetic.Zero;
            if (velocityBias != null)
                velocityBias.Value = Arithmetic.Zero;
        }

        public class Manager
            : Integrator<T>.Manager<Euler<T>>
        {
            public Manager(Scene scene, NinjectGame game)
                : base(scene.GetService<ProcessService>(), game, true)
            {
            }

            protected override void PrepareUpdate(float elapsedTime)
            {
            }

            protected override void Update(Euler<T> behaviour, float elapsedTime)
            {
                behaviour.Integrate(elapsedTime);
            }
        }
    }
}
