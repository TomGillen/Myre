﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the System.Collections.IList interface.
    /// </summary>
    public static class IListExtensions
    {
        /// <summary>
        /// Sorts the list using intersion sort. This is usually slower than List.Sort and Array.Sort, but is stable.
        /// Worst case O(n^2).
        /// Best case O(n) (already sorted list).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="comparison">The comparison.</param>
        public static void InsertionSort<T>(this IList<T> list, Comparison<T> comparison)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (comparison == null)
                throw new ArgumentNullException("comparison");

            int count = list.Count;
            for (int j = 1; j < count; j++)
            {
                T key = list[j];

                int i = j - 1;
                for (; i >= 0 && comparison(list[i], key) > 0; i--)
                {
                    list[i + 1] = list[i];
                }
                list[i + 1] = key;
            }
        }

        /// <summary>
        /// Sorts the list using intersion sort. This is usually slower than List.Sort and Array.Sort, but is stable.
        /// Worst case O(n^2).
        /// Best case O(n) (already sorted list).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="comparer">The comparer.</param>
        public static void InsertionSort<T>(this IList<T> list, IComparer<T> comparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            int count = list.Count;
            for (int j = 1; j < count; j++)
            {
                T key = list[j];

                int i = j - 1;
                for (; i >= 0 && comparer.Compare(list[i], key) > 0; i--)
                {
                    list[i + 1] = list[i];
                }
                list[i + 1] = key;
            }
        }
    }
}
