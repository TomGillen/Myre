using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Collections
{
    /// <summary>
    /// A sorted binary tree
    /// </summary>
    /// <typeparam name="T">Type of item to store in the tree.</typeparam>
    public class BinaryTree<T>
        : ICollection<T>
        where T : IComparable<T>
    {
        static Pool<Node> nodePool = new Pool<Node>();
        static bool lastUsedSuccessor = false;

        sealed class Node
            : IRecycleable
        {
            public T Value;
            public Node Left;
            public Node Right;
            public Node Parent;

            public bool Add(T item)
            {
                var comparison = item.CompareTo(Value);
                if (comparison > 0)
                {
                    if (Right == null)
                    {
                        Right = nodePool.Get();
                        Right.Value = item;
                        Right.Parent = this;
                        return true;
                    }
                    else
                        return Right.Add(item);
                }
                else if (comparison < 0)
                {
                    if (Left == null)
                    {
                        Left = nodePool.Get();
                        Left.Value = item;
                        Left.Parent = this;
                        return true;
                    }
                    else
                        return Left.Add(item);
                }

                return false;
            }

            public void Merge(Node source)
            {
                this.Add(source.Value);
                if (source.Left != null)
                    Merge(source.Left);
                if (source.Right != null)
                    Merge(source.Right);
            }

            public void Delete()
            {
                if (Left == null && Right == null)
                {
                    if (Parent != null)
                        Parent.ReplaceChild(this, null);

                    nodePool.Return(this);
                }
                else if (Left != null && Right != null)
                {
                    Node replacement = null;
                    if (lastUsedSuccessor)
                        replacement = Left.RightMostChild();
                    else
                        replacement = Right.LeftMostChild();
                    lastUsedSuccessor = !lastUsedSuccessor;

                    Value = replacement.Value;
                    replacement.Delete();
                }
                else
                {
                    var successor = Left ?? Right;
                    Value = successor.Value;
                    Left = successor.Left;
                    Right = successor.Right;

                    nodePool.Return(successor);
                }
            }

            public void Do(Action<T> action)
            {
                if (Left != null)
                    Left.Do(action);
                action(Value);
                if (Right != null)
                    Right.Do(action);
            }

            public void Clear()
            {
                if (Left != null)
                {
                    Left.Clear();
                    Left = null;
                }

                if (Right != null)
                {
                    Right.Clear();
                    Right = null;
                }

                nodePool.Return(this);
            }

            public Node Find(T value)
            {
                int comparison = value.CompareTo(Value);
                if (comparison == 0)
                    return this;
                else if (comparison > 0 && Right != null)
                    return Right.Find(value);
                else if (comparison < 0 && Left != null)
                    return Left.Find(value);
                else
                    return null;
            }

            public Node LeftMostChild()
            {
                if (Left == null)
                    return this;
                else
                    return Left.LeftMostChild();
            }

            public Node RightMostChild()
            {
                if (Right == null)
                    return this;
                else
                    return Right.RightMostChild();
            }

            #region IRecyclable Members

            public void Recycle()
            {
                Value = default(T);
                Left = null;
                Right = null;
                Parent = null;
            }

            #endregion

            internal bool AnyContainedIn(BinaryTree<T> binaryTree)
            {
                if (Left != null && Left.AnyContainedIn(binaryTree))
                    return true;
                if (Right != null && Right.AnyContainedIn(binaryTree))
                    return true;
                return binaryTree.Contains(Value);
            }

            private void ReplaceChild(Node current, Node replacement)
            {
                if (Left == current)
                    Left = replacement;
                if (Right == current)
                    Right = replacement;
            }
        }

        Node root;
        int count;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryTree&lt;T&gt;"/> class.
        /// </summary>
        public BinaryTree()
        {
        }

        /// <summary>
        /// Does the specified action on each value in the tree in order.
        /// </summary>
        /// <param name="action">The action.</param>
        public void Do(Action<T> action)
        {
            if (root != null)
                root.Do(action);
        }

        #region ICollection<T> Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Add(T item)
        {
            if (root == null)
            {
                root = new BinaryTree<T>.Node() { Value = item };
                count++;
            }
            else
            {
                if (root.Add(item))
                    count++;
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Clear()
        {
            if (root != null)
            {
                root.Clear();
                root = null;
                count = 0;
            }
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            if (root == null)
                return false;
            else
                return root.Find(item) != null;
        }

        /// <summary>
        /// Copies this binary treeto the specified array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (array.Length - arrayIndex < count)
                throw new ArgumentException("Not enough space in array to copy.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");

            if (root != null)
                FillArray(root, array, ref arrayIndex);
        }

        private void FillArray(Node n, T[] array, ref int indexOffset)
        {
            if (n.Left != null)
                FillArray(n.Left, array, ref indexOffset);

            array[indexOffset] = n.Value;
            indexOffset++;

            if (n.Right != null)
                FillArray(n.Right, array, ref indexOffset);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get { return count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public bool Remove(T item)
        {
            if (root == null)
                return false;
            else
            {
                Node node = root.Find(item);

                if (node == null)
                    return false;

                if (root == node && root.Left == null && root.Right == null)
                {
                    root.Delete();
                    root = null;
                }
                else
                    node.Delete();

                count--;
                return true;
            }
        }
        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            T[] items = new T[count];
            CopyTo(items, 0);
            return (items as IEnumerable<T>).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Determines whether this instance contains any of the values which are in the specified binary tree.
        /// </summary>
        /// <param name="binaryTree">The binary tree.</param>
        /// <returns>
        /// 	<c>true</c> if this instance contains any of the values which are in the specified binary tree; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsAny(BinaryTree<T> binaryTree)
        {
            if (root == null)
                return false;
            return root.AnyContainedIn(binaryTree);
        }
    }
}
