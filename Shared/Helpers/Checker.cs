using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using Shared.Extensions;

namespace Shared.Helpers
{
    public static class Checker
    {
        #region Constants

        private const string EXCEPTION_ARGUMENT_CannotEquals = "'{0}' cannot equals '0'";
        private const string EXCEPTION_VALUE_CannotEquals = "Value cannot be less or equals '0'";

        #endregion

        #region Fields

        private static Dictionary<Assembly, Func<string, Exception>> _AssemblyExceptionTypes;

        #endregion

        #region Methods

        #region Public

        public static void AreEqual<T>(T expected, T actual)
        {
            if (!expected.Equals(actual))
            {
                ThrowArgumentException(string.Format("Expected value '{0}' but actual is '{1}'", expected, actual));
            }
        }

        public static void AreNotEqual<T>(T notExpected, T actual)
        {
            if (notExpected.Equals(actual))
            {
                ThrowArgumentException(string.Format("Two values cannot be equal to '{0}'", notExpected));
            }
        }

        public static void GreaterZero(double value, string name)
        {
            if (value <= 0)
            {
                ThrowArgumentException(string.Format(EXCEPTION_ARGUMENT_CannotEquals, name));
            }
        }

        public static void GreaterZero(double value)
        {
            if (value <= 0)
            {
                ThrowArgumentException(EXCEPTION_VALUE_CannotEquals);
            }
        }

        public static void GreaterZero(int value, string name)
        {
            if (value <= 0)
            {
                ThrowArgumentException(string.Format(EXCEPTION_ARGUMENT_CannotEquals, name));
            }
        }

        public static void GreaterZero(int value)
        {
            if (value <= 0)
            {
                ThrowArgumentException(EXCEPTION_VALUE_CannotEquals);
            }
        }

        public static void GreaterZero(long value, string name)
        {
            if (value <= 0)
            {
                ThrowArgumentException(string.Format(EXCEPTION_ARGUMENT_CannotEquals, name));
            }
        }

        public static void GreaterZero(long value)
        {
            if (value <= 0)
            {
                ThrowArgumentException(EXCEPTION_VALUE_CannotEquals);
            }
        }

        public static void IsNotTrue(bool value)
        {
            if (value)
            {
                ThrowArgumentException("Argument cannot be 'true'.");
            }
        }

        public static void IsNotTrue(bool value, string name)
        {
            if (value)
            {
                ThrowArgumentException(string.Format("'{0}' cannot be 'true'.", name));
            }
        }

        public static void IsNull(object value)
        {
            /*   А еще здесь можно написать 
             *   Contract.Requires<ArgumentNullException>(value != null)
             *   может попозже и заменим
             * 
             */
            if (value.IsNotNull())
            {
                ThrowArgumentException("Value should be NULL");
            }

            //Contract.Requires<ArgumentNullException>(value == null);

            //Contract.EndContractBlock();
        }

        public static void IsTrue(bool value)
        {
            if (!value)
            {
                ThrowArgumentException("Argument cannot be 'false'.");
            }
        }

        public static void IsTrue(bool value, string name)
        {
            if (!value)
            {
                ThrowArgumentException(string.Format("'{0}' cannot be 'false'.", name));
            }
        }

        public static void NotNull(object value, string name)
        {
            if (value.IsNull())
            {
                ThrowArgumentNullException(name);
            }

            Contract.EndContractBlock();
        }

        //
        //
        public static void NotNull(object value)
        {
            /*   А еще здесь можно написать 
             *   Contract.Requires<ArgumentNullException>(value != null)
             *   может попозже и заменим
             * 
             */
            if (value.IsNull())
            {
                ThrowArgumentNullException("value");
            }

            //Contract.Requires<ArgumentNullException>(value == null);

            //Contract.EndContractBlock();
        }

        
        
        public static void NotNullOrEmpty(string value)
        {
            if (value.IsNullOrEmpty())
            {
                ThrowArgumentException("Value cannot be null or empty.");
            }
        }

        
        
        public static void NotNullOrEmpty(string value, string name)
        {
            if (value.IsNullOrEmpty())
            {
                ThrowArgumentException(string.Format("'{0}' cannot be null or empty.", name));
            }

            Contract.EndContractBlock();
        }

        public static void NullOrEmpty(string value)
        {
            if (value.IsNotNullOrEmpty())
            {
                ThrowArgumentException("Value must be null or empty.");
            }
        }

        public static void NullOrEmpty(string value, string name)
        {
            if (value.IsNotNullOrEmpty())
            {
                ThrowArgumentException(string.Format("'{0}' must be null or empty.", name));
            }
        }

        #endregion

        #region Private

        private static void ThrowArgumentException(string message)
        {
            throw new ArgumentException(message);
        }

        private static void ThrowArgumentNullException(string message)
        {
            throw new ArgumentNullException(message);
        }

        #endregion

        #endregion
    }
}
