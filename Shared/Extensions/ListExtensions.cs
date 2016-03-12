using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared.Extensions
{
    public static class ListExtensions
    {
        #region Methods

        #region Public

        public static void AddRange<T>(this IList<T> This, IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                This.Add(item);
            }
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static IEnumerable<T> Clone<T>(this IEnumerable<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static T BinarySearch<T>(this IList<T> list, Func<T, int> predicate) where T : class
        {
            int index = BinarySearchIndex(list, predicate);            

            return index >= 0 ? list[index] : null;
        }

        /// <summary>
        /// Returns true when found.
        /// </summary>
        public static bool BinarySearchFound<T>(this IList<T> list, Func<T, int> predicate)
        {
            int index = BinarySearchIndex(list, 0, list.Count - 1, predicate);

            return index >= 0;
        }

        /// <summary>
        /// Returns index of found item or -1 when not found.
        /// </summary>
        public static int BinarySearchIndex<T>(this IList<T> list, Func<T, int> predicate)
        {
            return BinarySearchIndex(list, 0, list.Count - 1, predicate);
        }

        /// <summary>
        /// Returns index of found item or -1 when not found.
        /// </summary>
        public static int BinarySearchIndex<T>(this IList<T> list, int indexFrom, int indexTo, Func<T, int> predicate)
        {
            int half = (indexTo - indexFrom) / 2;

            if (half > 0)
            {
                int index = indexFrom + half;
                int result = predicate(list[index]);

                if (result == 0)
                {
                    return index;
                }

                // if list[index] > target, we must go left
                if (result == 1)
                {
                    return BinarySearchIndex(list, indexFrom, index - 1, predicate);
                }

                return BinarySearchIndex(list, index + 1, indexTo, predicate);
            }

            for (int i = indexFrom; i <= indexTo;i++)
            {
                if (predicate(list[i]) == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion

        #endregion
    }
}
