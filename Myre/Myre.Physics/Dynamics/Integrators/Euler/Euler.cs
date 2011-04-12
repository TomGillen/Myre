﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Physics.Dynamics.Integrators.Arithmetic;
using Myre.Entities;
using Myre.Entities.Services;

namespace Myre.Physics.Dynamics.Integrators
{
    public class Euler<T>
        :Integrator<T>
    {
        public Euler(string position, string velocity, string acceleration, string velocityBias, Arithmetic<T> arithmetic)
            :base(position, velocity, acceleration, velocityBias, arithmetic)
        {

        }

        protected void Integrate(float deltaTime)
        {
            velocity.Value = Arithmetic.Add(velocity.Value, acceleration.Value);

            var deltaPosition = Arithmetic.Multiply(Arithmetic.Add(velocity.Value, velocityBias.Value), deltaTime);
            position.Value = Arithmetic.Add(position.Value, deltaPosition);

            acceleration.Value = Arithmetic.Zero;
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
