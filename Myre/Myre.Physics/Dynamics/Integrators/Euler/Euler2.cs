﻿using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;
using Myre.Physics.Dynamics.Integrators.Arithmetic;

namespace Myre.Physics.Dynamics.Integrators
{
    [DefaultManager(typeof(Manager))]
    public class Euler2
        : Euler<Vector2>
    {
        public Euler2(string position, string velocity, string acceleration, string velocityBias)
            : base(position, velocity, acceleration, velocityBias, new Arithmetic2())
        {
        }
    }
}
