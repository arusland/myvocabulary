using System;
using System.Globalization;
using Shared.Helpers;

namespace Shared.Extensions
{
    public static class StringExtensions
    {
        #region Methods

        #region Public

        public static bool Compare(this string str, string value, bool ignoreCase)
        {
            return String.Compare(str, value, ignoreCase) == 0;
        }

        public static string GetShort(this string str)
        {
            if (str.IsNull())
            {
                throw new ArgumentNullException("str");
            }

            string result = str;
            if (str.Length > 7)
            {
                result = string.Format("{0}...", str.Substring(0, 7));
            }

            return result;
        }

        public static bool IsEmpty(this string str)
        {
            if (str.IsNull())
            {
                throw new ArgumentNullException("str");
            }

            return str == string.Empty;
        }

        public static bool IsNotEmpty(this string str)
        {
            return !str.IsEmpty();
        }

        public static bool IsNotNullOrEmpty(this string str)
        {
            return str.IsNotNull() && str.IsNotEmpty();
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return !str.IsNotNullOrEmpty();
        }

        public static string Lower(this string value)
        {
            Checker.NotNull(value);

            return value.ToLower(CultureInfo.CurrentCulture);
        }

        public static T ParseEnum<T>(this string value)
        {
            Checker.NotNullOrEmpty(value, "value");

            try
            {
                return Enum.Parse(typeof(T), value).To<T>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("'{0}' cannot be parsed as {1}", value, typeof(T)), ex);
            }
        }

        public static string Upper(this string value)
        {
            Checker.NotNull(value);

            return value.ToUpper(CultureInfo.CurrentCulture);
        }

        public static string UpperFirstChar(this string value)
        {
            Checker.NotNull(value);

            if (value.Length > 0)
            {
                return value[0].ToString().ToUpper(CultureInfo.CurrentCulture) + value.Substring(1, value.Length - 1);
            }

            return string.Empty;
        }

        #endregion

        #endregion
    }
}