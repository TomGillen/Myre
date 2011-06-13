using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Myre.Physics.Collisions
{
    public class CollisionDetector
    {
        private List<Collision> collisions;
        private ReadOnlyCollection<Collision> collisionsWrapper;
        private List<Geometry> geometry;
        private SatTester tester;

        public ReadOnlyCollection<Collision> Collisions
        {
            get { return collisionsWrapper; }
        }

        public CollisionDetector()
        {
            collisions = new List<Collision>();
            collisionsWrapper = new ReadOnlyCollection<Collision>(collisions);
            geometry = new List<Geometry>();
            tester = new SatTester();
        }

        public void Add(Geometry geom)
        {
            geometry.Add(geom);
        }

        public bool Remove(Geometry geom)
        {
            if (geometry.Remove(geom))
            {
                for (int i = collisions.Count - 1; i >= 0; i--)
                {
                    var collision = collisions[i];
                    if (collision.A == geom || collision.B == geom)
                    {
                        collision.Dispose();
                        collisions.RemoveAt(i);
                    }
                }

                return true;
            }

            return false;
        }

        public void Update()
        {
            DoBroadphase();
            DoNarrowphase();
            RemoveDeadCollisions();
        }

        private void DoBroadphase()
        {
            // TODO: Implement Sweep and prune ;)
            for (int i = 0; i < geometry.Count; i++)
            {
                for (int j = i + 1; j < geometry.Count; j++)
                {
                    var a = geometry[i];
                    var b = geometry[j];

                    if (a.Body.Mass == float.PositiveInfinity && b.Body.Mass == float.PositiveInfinity
                        && a.Body.InertiaTensor == float.PositiveInfinity && b.Body.InertiaTensor == float.PositiveInfinity)
                        continue;

                    if (a.Body.Sleeping && b.Body.Sleeping)
                        continue;

                    if (a.Bounds.Intersects(b.Bounds))
                    {
                        if (!a.collidingWith.Contains(b))
                        {
                            var collision = Collision.Create(geometry[i], geometry[j]);
                            collisions.Add(collision);
                        }
                    }
                }
            }
        }

        private void DoNarrowphase()
        {
            for (int i = 0; i < collisions.Count; i++)
            {
                var collision = collisions[i];
                collision.FindContacts(tester);

                if (collision.Contacts.Count > 0)
                {
                    if (!collision.B.Body.IsStatic && !collision.B.Body.Sleeping)
                        collision.A.Body.Sleeping = false;

                    if (!collision.A.Body.IsStatic && !collision.A.Body.Sleeping)
                        collision.B.Body.Sleeping = false;
                }

                //DynamicPhysics activatedBody;
                //if (collisions[i].ShouldActivateBody(out activatedBody))
                //{
                //    activatedBody.Sleeping = false;

                //    for (int j = i - 1; j >= 0; j--)
                //    {
                //        if (collisions[j].A.Body == activatedBody || collisions[j].B.Body == activatedBody)
                //            collisions[j].FindContacts(tester);
                //    }
                //}
            }
        }

        private void RemoveDeadCollisions()
        {
            for (int i = collisions.Count - 1; i >= 0; i--)
            {
                if (collisions[i].IsDead)
                {
                    collisions[i].Dispose();
                    collisions.RemoveAt(i);
                }
            }
        }
    }
}
