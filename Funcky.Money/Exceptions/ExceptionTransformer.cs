using System;

namespace Funcky
{
    internal static class ExceptionTransformer<TException>
        where TException : Exception
    {
        public static T Transform<T>(Func<T> func, Action<TException> throwNewException)
        {
            try
            {
                return func();
            }
            catch (TException exception)
            {
                throwNewException(exception);
                throw new Exception("throwNewException should throw an exception, but didn't");
            }
        }

        public static void Transform<T>(Action action, Action<TException> throwNewException)
        {
            try
            {
                action();
            }
            catch (TException exception)
            {
                throwNewException(exception);
            }
        }
    }
}
