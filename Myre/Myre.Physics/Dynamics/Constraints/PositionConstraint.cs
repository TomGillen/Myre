using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Myre.Entities;
using Microsoft.Xna.Framework;

namespace Myre.Physics.Dynamics.Constraints
{
    public class PositionConstraint
        :Behaviour
    {
        private Property<Vector3> position;
        private Property<float> mass;
        private Property<float> inverseMass;

        public PositionConstraint()
        {
        }

        public override void CreateProperties(Entity.InitialisationContext context)
        {
            position = context.GetProperty<Vector3>(PhysicsProperties.POSITION);
            mass = context.GetProperty<float>(PhysicsProperties.MASS);
            inverseMass = context.GetProperty<float>(InverseMassCalculator.INVERSE_MASS);

            base.CreateProperties(context);
        }

        public Vector3 Constrain(Vector3 otherPosition, float otherInverseMass, float restingDistance, float stiffness)
        {
            Vector3 delta = position.Value - otherPosition;
            float dist = delta.Length();

            if (dist == 0)
                return Vector3.Zero;

            var multiplier = (delta * stiffness * ((restingDistance - dist) / dist)) / (inverseMass.Value + otherInverseMass);

            position.Value += inverseMass.Value * multiplier;

            return -multiplier;
        }
    }
}
