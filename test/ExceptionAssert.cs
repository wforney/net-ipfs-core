namespace Ipfs;

/// <summary>
/// Asserting an <see cref="Exception"/>.
/// </summary>
public static class ExceptionAssert
{
    /// <summary>
    /// Asserts that the <paramref name="action"/> throws an exception of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the expected exception.
    /// </typeparam>
    /// <param name="action">
    /// The action that is expected to throw the exception.
    /// </param>
    /// <param name="expectedMessage">
    /// The expected exception message.
    /// </param>
    /// <returns>
    /// The exception that was thrown.
    /// </returns>
    /// <exception cref="Exception">
    /// No exception of type <typeparamref name="T"/> was thrown.
    /// </exception>
    public static T Throws<T>(Action action, string? expectedMessage = null) where T : Exception
    {
        try
        {
            action();
        }
        catch (AggregateException e)
        {
            T? match = e.InnerExceptions.OfType<T>().FirstOrDefault();
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

        // The compiler doesn't know that Assert.Fail will always throw an exception
        throw new Exception();
    }

    /// <summary>
    /// Asserts that the <paramref name="func"/> throws an exception of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the expected exception.
    /// </typeparam>
    /// <typeparam name="TTest">
    /// The type of the return value from <paramref name="func"/>.
    /// </typeparam>
    /// <param name="func">
    /// The function that is expected to throw the exception.
    /// </param>
    /// <returns>
    /// The exception that was thrown.
    /// </returns>
    /// <remarks>
    /// Avoids analyzer recommendations related to unused values.
    /// </remarks>
    public static T Throws<T, TTest>(Func<TTest> func) where T : Exception => Throws<T>(() => { TTest? _ = func(); });

    /// <summary>
    /// Asserts that the <paramref name="func"/> throws an exception of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the expected exception.
    /// </typeparam>
    /// <typeparam name="TTest">
    /// The type of the return value from <paramref name="func"/>.
    /// </typeparam>
    /// <param name="func">
    /// The function that is expected to throw the exception.
    /// </param>
    /// <param name="expectedMessage">
    /// The expected exception message.
    /// </param>
    /// <returns>
    /// The exception that was thrown.
    /// </returns>
    /// <remarks>
    /// Avoids analyzer recommendations related to unused values.
    /// </remarks>
    public static T Throws<T, TTest>(Func<TTest> func, string? expectedMessage = null) where T : Exception => Throws<T>(() => { TTest? _ = func(); }, expectedMessage);
}
