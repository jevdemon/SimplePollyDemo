using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace PollyDemo
{
    class PolicyMaker
    {
        public Policy CreatePolicy()
        {
            var policy = Policy.Handle<Exception>() // general exception handler - normally we'd handle a specific exception
                .WaitAndRetryAsync(6, // Retry up to a maximum of 6 times (this would normally be configurable)
                attempt => TimeSpan.FromSeconds(0.1 * Math.Pow(2, attempt)), // exponential backoff (this would normally be configurable)
                onRetry: (exception, calculatedWaitDuration, attempt, context) =>  // on retry run the delegate below (can also invoke a method)
                {
                    // if our delegate was called it means a request failed and we are retrying
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\tException: " + exception.Message);
                    Console.WriteLine("\tRetry #" + attempt);
                    Console.WriteLine("\t... automatically delaying for " + calculatedWaitDuration.TotalMilliseconds + "ms.");
                    Console.ForegroundColor = ConsoleColor.White;

                    // check to see if we have a retry key in our context and set it to our retry count
                    // this enables callers using this policy to see how many times we had to retry
                    if (context.ContainsKey("retry"))
                        (context["retry"] as RetryCounter).Value = attempt;
                });
            return policy;
        }
    }
}
