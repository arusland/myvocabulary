using System;

namespace Shared.Extensions
{
    public static class GenericExtensions
    {
        #region Methods

        #region Public

        public static T Duck<T>(this T value, Action<T> handler)
        {
            handler(value);

            return value;
        }

        #endregion

        #endregion
    }
}
