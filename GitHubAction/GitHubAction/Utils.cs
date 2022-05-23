namespace GitHubAction
{
    public static class Utils
    {
        /// <summary>
        /// Executes a given function and applies the validation to it with a backing off retry mechanism with a configurable back off delay, maximum back off delay and timeout.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="funcAsync">The function to execute.</param>
        /// <param name="validation">The validation to do on the response.</param>
        /// <param name="onBackOff">An action executed every back off with the amount of seconds it will back off as input argument. Could be used for logging.</param>
        /// <param name="backOffDelay">Initial delay which will increase with the back off mechanism.</param>
        /// <param name="maxBackOffDelay">The maximum back off delay.</param>
        /// <param name="untilTimeout">The timeout for when we don't care anymore about the response. Will throw a <see cref="TimeoutException"/>.</param>
        /// <returns><typeparamref name="T"/></returns>
        /// <remarks>
        ///     - This can be a long running operation depending on the input parameters (<paramref name="untilTimeout"/>).
        /// </remarks>
        /// <exception cref="TimeoutException">When the function response wasn't validated successfully within the specified <paramref name="untilTimeout"/>.</exception>
        public static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> funcAsync,
            Func<T, bool> validation,
            Action<int> onBackOff,
            TimeSpan backOffDelay,
            TimeSpan maxBackOffDelay,
            TimeSpan untilTimeout)
        {
            // Execute at least once immediately
            var result = await funcAsync();

            while (!validation(result) && untilTimeout > backOffDelay)
            {
                onBackOff((int)backOffDelay.TotalSeconds);
                untilTimeout = untilTimeout.Subtract(backOffDelay);
                await Task.Delay((int)backOffDelay.TotalMilliseconds);
                var nextBackOffDelay = backOffDelay.Multiply(2);
                backOffDelay = nextBackOffDelay < maxBackOffDelay ? nextBackOffDelay : maxBackOffDelay;

                result = await funcAsync();
            }

            // Response could be valid
            if (validation(result))
            {
                return result;
            }

            // Response not valid -> Would go over timeout with this back off so just take the resulting time untilTimeout if any
            if (untilTimeout > TimeSpan.Zero)
            {
                onBackOff((int)untilTimeout.TotalSeconds);
                await Task.Delay((int)untilTimeout.TotalMilliseconds);
                result = await funcAsync();
            }

            // Response could be valid
            if (validation(result))
            {
                return result;
            }

            // Still no valid response, so we stop because of timeout
            throw new TimeoutException();
        }
    }
}
