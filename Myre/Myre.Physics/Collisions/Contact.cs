using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.Physics.Collisions
{
    public struct Contact
    {
        public struct ContactID
        {
            public Geometry Geometry;
            public int Feature;

            public override int GetHashCode()
            {
                return Geometry.GetHashCode() ^ Feature.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is ContactID)
                    return Equals((ContactID)obj);
                else
                    return base.Equals(obj);
            }

            public bool Equals(ContactID obj)
            {
                return this.Geometry == obj.Geometry
                    && this.Feature == obj.Feature;
            }
        }

        public Vector2 Position;
        public ContactID ID;
        internal float massNormal;
        internal float massTangent;
        internal float normalVelocityBias;
        internal float bounceVelocity;
        internal float normalImpulse;
        internal float tangentImpulse;
        internal float normalImpulseBias;

        public Contact(Vector2 position, Geometry geometry, int feature)
        {
            this.Position = position;
            this.ID = new ContactID() { Geometry = geometry, Feature = feature };
            this.massNormal = 0;
            this.massTangent = 0;
            this.normalVelocityBias = 0;
            this.bounceVelocity = 0;
            this.normalImpulse = 0;
            this.tangentImpulse = 0;
            this.normalImpulseBias = 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is Contact)
                return Equals((Contact)obj);
            else
                return base.Equals(obj);
        }

        public bool Equals(Contact c)
        {
            return this.ID.Equals(c.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }
}
