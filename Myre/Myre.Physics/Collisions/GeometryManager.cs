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
            : BehaviourManager<Geometry>, IProcess
        {
            private CollisionDetector collisionDetector;

            public float AllowedPenetration { get; set; }
            public float BiasFactor { get; set; }
            public int Iterations { get; set; }

            public ReadOnlyCollection<Collision> Collisions
            {
                get { return collisionDetector.Collisions; }
            }

            public ReadOnlyCollection<Geometry> Geometry
            {
                get;
                private set;
            }

            bool IProcess.IsComplete
            {
                get { return false; }
            }

            public Manager(Scene scene)
            {
                scene.GetService<ProcessService>().Add(this);
                collisionDetector = new CollisionDetector();
                Geometry = new ReadOnlyCollection<Geometry>(Behaviours);

                AllowedPenetration = 1f;
                BiasFactor = 0.05f;
                Iterations = 15;
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

            public void Update(float time)
            {
                collisionDetector.Update();

                var inverseDT = 1f / time;
                for (int i = 0; i < collisionDetector.Collisions.Count; i++)
                    collisionDetector.Collisions[i].Prepare(AllowedPenetration, BiasFactor, inverseDT);

                for (int i = 0; i < Iterations; i++)
                {
                    for (int j = 0; j < collisionDetector.Collisions.Count; j++)
                    {
                        //System.Diagnostics.Debug.WriteLine("Collision:{0}", j);
                        collisionDetector.Collisions[j].Iterate();
                    }
                }
            }
        }
    }
}
