using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Myre.Entities;
using System.Collections.ObjectModel;

namespace Myre.Physics.Collisions
{
    [DefaultManager(typeof(Manager))]
    public partial class Geometry
    {
        public class Manager
            : BehaviourManager<Geometry>, ICollisionResolver
        {
            private CollisionDetector collisionDetector;

            public ReadOnlyCollection<Collision> Collisions
            {
                get { return collisionDetector.Collisions; }
            }

            public ReadOnlyCollection<Geometry> Geometry
            {
                get;
                private set;
            }

            public Manager(Scene scene)
            {
                collisionDetector = new CollisionDetector();
                Geometry = new ReadOnlyCollection<Geometry>(Behaviours);
            }

            public override void Add(Geometry behaviour)
            {
                collisionDetector.Add(behaviour);
                base.Add(behaviour);
            }

            public override bool Remove(Geometry behaviour)
            {
                collisionDetector.Remove(behaviour);
                return base.Remove(behaviour);
            }

            public void Update(float time, float allowedPenetration, float biasFactor, int iterations)
            {
                collisionDetector.Update();

                var inverseDT = 1f / time;
                for (int i = 0; i < collisionDetector.Collisions.Count; i++)
                    collisionDetector.Collisions[i].Prepare(allowedPenetration, biasFactor, inverseDT);

                for (int i = 0; i < iterations; i++)
                {
                    for (int j = 0; j < collisionDetector.Collisions.Count; j++)
                        collisionDetector.Collisions[j].Iterate();
                }
            }
        }
    }
}
