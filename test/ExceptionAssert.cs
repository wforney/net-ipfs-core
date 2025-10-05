using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ipfs
{
    /// <summary>
    ///   Asserting an <see cref="Exception"/>.
    /// </summary>
    public static class ExceptionAssert
    {
        public static T Throws<T>(Action action, string? expectedMessage = null) where T : Exception
        {
            try
            {
                action();
            }
            catch (AggregateException e)
            {
                var match = e.InnerExceptions.OfType<T>().FirstOrDefault();
                if (match is not null)
                {
                    if (expectedMessage is not null)
                    {
                        Assert.AreEqual(expectedMessage, match.Message, "Wrong exception message.");
                    }

                    return match;
                }

                throw;
            }
            catch (T e)
            {
                if (expectedMessage is not null)
                {
                    Assert.AreEqual(expectedMessage, e.Message);
                }

                return e;
            }
            Assert.Fail($"Exception of type {typeof(T)} should be thrown.");

            //  The compiler doesn't know that Assert.Fail will always throw an exception
            throw new Exception();
        }

        // Avoids analyzer recommendations related to unused values.
        public static T Throws<T, TTest>(Func<TTest> func) where T : Exception =>
            Throws<T>(() => { var _ = func(); });

        // Avoids analyzer recommendations related to unused values.
        public static T Throws<T, TTest>(Func<TTest> func, string? expectedMessage = null) where T : Exception =>
            Throws<T>(() => { var _ = func(); }, expectedMessage);
    }
}
