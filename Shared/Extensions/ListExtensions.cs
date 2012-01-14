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

        #endregion

        #endregion
    }
}
