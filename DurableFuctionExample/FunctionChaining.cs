using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace RyansDurableFuctionExample
{
    public static class FunctionChaining
    {
        [FunctionName("FunctionChaining")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("FunctionChaining_Hello", "MDG"));
            outputs.Add(await context.CallActivityAsync<string>("FunctionChaining_Hello", "Cadastro"));
            outputs.Add(await context.CallActivityAsync<string>("FunctionChaining_Hello", "Promax"));

            // returns ["Hello MDG!", "Hello Cadastro!", "Hello Promax!"]
            return outputs;
        }

        [FunctionName("FunctionChaining_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("FunctionChaining_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("FunctionChaining", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}