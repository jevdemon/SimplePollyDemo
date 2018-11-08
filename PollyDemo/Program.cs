using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace PollyDemo
{
    class Program
    {
       // Simulate limited service availability with a Fiddler Autoresponder rule:
       // %50example.com
       // 404_Plain.dat

        static void Main()
        {
            while (true)
            {
                Task t = new Task(GetRequest);
                t.Start();
                Console.WriteLine("Sending request...");

                if (Console.ReadLine().ToLower() == "q")
                    break;
                else
                    Console.Clear();
            }
        }
        static async void GetRequest()
        {
            string getUrl = "http://example.com";
            var httpClient = new HttpClient();          

            // create our retry policy using a separate class
            var policy = new PolicyMaker().CreatePolicy();

            // create a Polly Context (Dictionary) to pass into the retry delegate 
            // the retry delegate will set the retry count in Polly Context and pass it back to us 
            // Polly Context format is {string (id), string, object}
            Polly.Context context = new Polly.Context(Guid.NewGuid().ToString());

            // RetryCounter is a simple object added to our Polly Context for tracking retries
            RetryCounter retryCounter = new RetryCounter();
            retryCounter.Value = 0;
            context.Add("retry", retryCounter);

            // keep track of successful requests
            int totalRequests = 0;

            try
            {
                using (var client = new HttpClient())
                {
                    totalRequests++;
                    // Retry the following call according to the policy - 15 times.
                    await policy.ExecuteAsync(async token =>
                        {
                        // This code is executed within the Policy 

                            // Make a request and get a response
                            string msg = await client.GetStringAsync(getUrl);
                            // Display success message 
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("GET was successful");
                            Console.ForegroundColor = ConsoleColor.White;

                    // our Polly context is passed into the delegate below
                    }, context);
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Request " + (context["retry"] as RetryCounter).Value + " eventually failed with: " + e.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
            finally
            {
                Console.WriteLine("\nWe retried " + (context["retry"] as RetryCounter).Value + " times.");
                Console.WriteLine("\nHit ENTER to try again\n\tOR\nType Q and hit ENTER to quit");
            }

        }
    }
}
