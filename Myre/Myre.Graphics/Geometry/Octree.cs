using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Collections;

namespace Myre.Graphics.Geometry
{
    public class Octree
    {
        const int SPLIT_THRESHOLD = 20;

        class Node
        {
            public BoundingBox Bounds { get; set; }
            public List<ICullable> Items { get; private set; }
            public List<Node> Children { get; private set; }

            public Node()
            {
                Items = new List<ICullable>();
                Children = new List<Node>();
            }

            public void Add(ICullable item)
            {
                //var bounds = item.BoundingBox.AxisAlignedBounds;

                bool insertedIntoChild = false;
                for (int i = 0; i < Children.Count; i++)
                {
                    if (Children[i].Bounds.Contains(item.Bounds) == ContainmentType.Contains)
                    {
                        Children[i].Add(item);
                        insertedIntoChild = true;
                        break;
                    }
                }

                if (!insertedIntoChild)
                    Items.Add(item);

                if (Children.Count == 0 && Items.Count > SPLIT_THRESHOLD)
                {
                    //var min = -Vector3.One;//Bounds.Min;
                    //var max = Vector3.One;//Bounds.Max;
                    //var centre = (min + max) / 2;
                    //Children.Add(new Node() { Bounds = new BoundingBox(new Vector3(min.X, min.Y, min.Z), new Vector3(centre.X, centre.Y, centre.Z)) });
                    //Children.Add(new Node() { Bounds = new BoundingBox(new Vector3(min.X, centre.Y, min.Z), new Vector3(centre.X, max.Y, centre.Z)) });
                    //Children.Add(new Node() { Bounds = new BoundingBox(new Vector3(centre.X, centre.Y, min.Z), new Vector3(max.X, max.Y, centre.Z)) });
                    //Children.Add(new Node() { Bounds = new BoundingBox(new Vector3(centre.X, min.Y, min.Z), new Vector3(max.X, centre.Y, centre.Z)) });
                    //Children.Add(new Node() { Bounds = new BoundingBox(new Vector3(min.X, min.Y, centre.Z), new Vector3(centre.X, centre.Y, max.Z)) });
                    //Children.Add(new Node() { Bounds = new BoundingBox(new Vector3(min.X, centre.Y, centre.Z), new Vector3(centre.X, max.Y, max.Z)) });
                    //Children.Add(new Node() { Bounds = new BoundingBox(new Vector3(centre.X, centre.Y, centre.Z), new Vector3(max.X, max.Y, max.Z)) });
                    //Children.Add(new Node() { Bounds = new BoundingBox(new Vector3(centre.X, min.Y, centre.Z), new Vector3(max.X, centre.Y, max.Z)) });

                    var min = Bounds.Min;
                    var size = (Bounds.Max - Bounds.Min) / 2;
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            for (int z = 0; z < 2; z++)
                            {
                                var positionOffset = size * new Vector3(x, y, z);
                                Children.Add(new Node() { Bounds = new BoundingBox(min + positionOffset, min + size + positionOffset) });
                            }
                        }
                    }


                    var items = new List<ICullable>(Items);
                    Items.Clear();
                    foreach (var i in items)
                    {
                        Add(i);
                    }
                }
            }

            public void Remove(ICullable item)
            {
                bool foundInChild = false;
                for (int i = 0; i < Children.Count; i++)
                {
                    if (Children[i].Bounds.Contains(item.Bounds) == ContainmentType.Contains)
                    {
                        Children[i].Remove(item);
                        foundInChild = true;
                        break;
                    }
                }

                if (!foundInChild)
                    Items.Remove(item);
            }

            public void Transfer(Node newRoot)
            {
                foreach (var item in Items)
                {
                    newRoot.Add(item);
                }

                foreach (var child in Children)
                {
                    child.Transfer(newRoot);
                }

                Items.Clear();
                Children.Clear();
                nodePool.Return(this);
            }

            public void Query(List<ICullable> results, BoundingVolume volume)
            {
                var containmentType = volume.Contains(Bounds);
                if (containmentType == ContainmentType.Intersects)
                {
                    foreach (var item in Items)
                    {
                        if (volume.Contains(item.Bounds) != ContainmentType.Disjoint)
                            results.Add(item);
                    }

                    foreach (var child in Children)
                    {
                        child.Query(results, volume);
                    }
                }
                else if (containmentType == ContainmentType.Contains)
                    AddAll(results);
            }

            private void AddAll(List<ICullable> results)
            {
                foreach (var item in Items)
                    results.Add(item);

                foreach (var child in Children)
                    child.AddAll(results);
            }
        }

        Node root;
        static Pool<Node> nodePool = new Pool<Node>();

        public Octree()
            : this(Vector3.One) 
        {
        }

        public Octree(Vector3 worldSize)
        {
            root = nodePool.Get();
            root.Bounds = new BoundingBox(-Vector3.One, Vector3.One);
        }

        public void Add(ICullable item)
        {
            var bounds = item.Bounds;
            if (root.Bounds.Contains(bounds) != ContainmentType.Contains)
            {
                BoundingBox newWorld = root.Bounds;
                do
                {
                    var extents = new Vector3(bounds.Radius + 1);
                    var newItemBounds = new BoundingBox(bounds.Center - extents, bounds.Center + extents);
                    newWorld = BoundingBox.CreateMerged(newWorld, newItemBounds);
                } while (newWorld.Contains(bounds) != ContainmentType.Contains);

                var newRoot = nodePool.Get();
                newRoot.Bounds = newWorld;
                root.Transfer(newRoot);
                root = newRoot;
            }

            root.Add(item);
        }

        public void Remove(ICullable item)
        {
            root.Remove(item);
        }

        public void Query(List<ICullable> results, BoundingVolume volume)
        {
            root.Query(results, volume);
        }
    }
}
