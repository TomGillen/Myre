using Myre.Entities;
using Myre.Entities.Services;
using Myre.Physics.Dynamics.Integrators.Arithmetic;
using Myre.Entities.Behaviours;
using Microsoft.Xna.Framework;
using System;

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

        private T previousVelocity;

        public Verlet(IntegratorProperties properties, Arithmetic<T> arithmetic)
            : base(properties, arithmetic)
        {
        }

        public void Integrate(float dT, float deltaTimeSquare)
        {
            ////velocity.Value = position.Value - previousPosition;
            //var velocityChange = Arithmetic.Multiply(Arithmetic.Subtract(velocity, previousVelocity), dT);
            //velocity.Value = Arithmetic.Add(Arithmetic.Subtract(position, previousPosition), velocityChange);

            //previousPosition = position.Value;
            
            ////position.Value = (position.Value + velocity.Value + velocityBias.Value) + acceleration.Value * deltaTimeSquare;
            //var sumPositionVelocityBias = Arithmetic.Add(Arithmetic.Add(position.Value, velocity.Value), (velocityBias == null ? Arithmetic.Zero : velocityBias.Value));
            //var accelerationTerm = Arithmetic.Multiply(acceleration.Value, deltaTimeSquare);
            //position.Value = Arithmetic.Add(sumPositionVelocityBias, accelerationTerm);

            //velocity.Value = Arithmetic.Subtract(position, previousPosition);
            //previousVelocity = velocity.Value;

            //acceleration.Value = Arithmetic.Zero;
            //if (velocityBias != null)
            //    velocityBias.Value = Arithmetic.Zero;
        }

        public override void SetPosition(T pos)
        {
            PreviousPosition = pos;
            base.SetPosition(pos);
        }

        public class Manager
            : BehaviourManager<Verlet<T>>, IIntegrator
        {
            private float dtSquare;

            public Manager(Scene scene, Game game)
            {
                if (!game.IsFixedTimeStep)
                    throw new Exception("The vertlet integrator requires the game uses a fixed timestep.");
            }

            public void UpdateVelocity(float elapsedTime)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var item = Behaviours[i];
                    var arithmetic = item.Arithmetic;

                    T velocity = item.velocity.Value;
                    T acceleration = item.acceleration.Value;

                    // TODO: calculate stuff

                    item.velocity.Value = velocity;
                }
            }

            public void UpdatePosition(float elapsedTime)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var item = Behaviours[i];
                    var arithmetic = item.Arithmetic;

                    T position = item.position.Value;
                    T velocity = item.velocity.Value;

                    // TODO: calculate stuff

                    item.position.Value = position;
                }
            }
        }
    }
}
