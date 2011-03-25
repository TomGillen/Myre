using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Entities;

namespace Myre.Physics.Dynamics
{
    public class InverseMassCalculator
        :Behaviour
    {
        public const String MASS = PropertyName.MASS;
        public const String INVERSE_MASS = PropertyName.INVERSE_MASS;

        public override void CreateProperties(Entities.Entity.InitialisationContext context)
        {
            Property<float> mass = context.CreateProperty<float>(MASS);
            Property<float> invMass = context.CreateProperty<float>(INVERSE_MASS);

            mass.PropertyChanged += _ =>
            {
                invMass.Value = 1 / mass.Value;
            };

            mass.Value = mass.Value;

            base.CreateProperties(context);
        }
    }
}
