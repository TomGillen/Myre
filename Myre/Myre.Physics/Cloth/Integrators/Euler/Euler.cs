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
        : Integrator<T>
    {
        public Euler(string position, string velocity, string acceleration, string velocityBias, Arithmetic<T> arithmetic)
            :base(position, velocity, acceleration, velocityBias, arithmetic)
        {

        }

        public Euler(IntegratorProperties properties, Arithmetic<T> arithmetic)
            : base(properties, arithmetic)
        {

        }

        public void IntegrateVelocity(float elapsedTime)
        {
            T velocity = this.velocity.Value;
            T acceleration = this.acceleration.Value;

            Arithmetic.Multiply(ref acceleration, elapsedTime, out acceleration);
            Arithmetic.Add(ref velocity, ref acceleration, out velocity);

            this.velocity.Value = velocity;
        }

        public void IntegratePosition(float elapsedTime)
        {
            T position = this.position.Value;
            T velocity = this.velocity.Value;

            if (this.velocityBias != null)
            {
                T bias = this.velocityBias.Value;
                Arithmetic.Add(ref velocity, ref bias, out velocity);
            }

            Arithmetic.Multiply(ref velocity, elapsedTime, out velocity);
            Arithmetic.Add(ref position, ref velocity, out position);

            this.position.Value = position;
        }

        public class Manager
            : BehaviourManager<Euler<T>>, IIntegrator
        {
            public void UpdateVelocity(float elapsedTime)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                    Behaviours[i].IntegrateVelocity(elapsedTime);
            }

            public void UpdatePosition(float elapsedTime)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                    Behaviours[i].IntegratePosition(elapsedTime);
            }
        }
    }
}
