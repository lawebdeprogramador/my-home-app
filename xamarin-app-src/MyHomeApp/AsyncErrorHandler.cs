using System;
using System.Diagnostics;

namespace MyHomeApp
{
    /// <summary>
    /// AsyncErrorHandler
    /// Install-Package AsyncErrorHandler.Fody
    /// Even with all the caching, retrying, and planning that we've put into the code, it will still fail at some point. We still want to make sure that when that happens, we handle it in a graceful manner.
    /// In our mobile apps, it's imperative that we use async/await as much as possible to ensure that we're not blocking the UI thread while we do things like make network requests.Handling exceptions from async methods can be tricky.
    /// Adding AsyncErrorHandler allows us to handle these exceptions in a global way, to ensure that they don't terminate our app.
    /// http://arteksoftware.com/resilient-network-services-with-xamarin/
    /// </summary>
    public static class AsyncErrorHandler
    {
        public static void HandleException(Exception exception)
        {
#if DEBUG
            Debug.WriteLine(exception);
#endif
            //Insights.Report(exception);
        }
    }
}
