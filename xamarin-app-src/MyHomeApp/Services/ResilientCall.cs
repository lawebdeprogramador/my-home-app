using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;

namespace MyHomeApp.Services
{
    public static class ResilientCall
    {
        public static async Task<PolicyResult<TReturn>> ExecuteWithRetry<TReturn>(Func<Task<TReturn>> action, int retryCount = 2)
        {
            return await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync
                (
                    retryCount: retryCount,
                    sleepDurationProvider: retryAttempt => retryAttempt.OnExponentially()
                )
                .ExecuteAndCaptureAsync(
                    async () => await action()
                ).ConfigureAwait(false);
        }

        private static TimeSpan OnExponentially(this int retryAttempt)
        {
            return TimeSpan.FromSeconds(Math.Pow(retryAttempt, 1));
        }
    }
}
