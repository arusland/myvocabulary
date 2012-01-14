using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shared.Extensions
{
    public static class IEnumerableExtensions
    {
        #region Methods

        #region Public

        public static void CallOnEach<T>(this IEnumerable<T> value, Action<T> handler)
        {
            foreach (var item in value.ToList())
            {
                handler(item);
            }
        }

        public static IEnumerable<TResult> SelectSubItems<TSource, TResult>(this IEnumerable<TSource> value, Func<TSource, IEnumerable<TResult>> selector)
        {
            foreach (var item in value)
            {
                foreach (var sel in selector(item))
                {
                    yield return sel;
                }
            }
        }

        public static string ToSeparatedString<TSource>(this IEnumerable<TSource> value, string seperator, Func<TSource, string> func)
        {
            StringBuilder result = new StringBuilder();

            foreach (var item in value)
            {
                if (result.Length > 0)
                {
                    result.Append(seperator);
                }
                result.Append(func(item));
            }

            return result.ToString();
        }

        #endregion

        #endregion
    }
}
