/* https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-cloud-backup?tabs=csharp
 * 
 * 
 * */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace RyansDurableFuctionExample
{
    public static class FanInFanOut
    {
        [FunctionName("FanInFanOut")]
        public static async Task<List<string>> RunOrchestrator(
           [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var inputs = new List<string>() { "MDG", "Cadastro", "Promax" };

            // Fan-out
            var tasks = new Task<string>[inputs.Count];
            for (int i = 0; i < inputs.Count; i++)
            {
                tasks[i] = context.CallActivityAsync<string>("FanInFanOut_Hello", inputs[i]);
            }

            await Task.WhenAll(tasks);

            return tasks.Select(t => t.Result).ToList();
        }

        [FunctionName("FanInFanOut_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"hello to {name}.");
            return $"Hello {name}!";
        }


        [FunctionName("FanInFanOut_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("FanInFanOut", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }

}