using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFuctionExample
{
    [FunctionName("TimerOrchestrator")]
    public static async Task<bool> Run([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(30);
        DateTime deadline = context.CurrentUtcDateTime.Add(timeout);

        using (var cts = new CancellationTokenSource())
        {
            Task activityTask = context.CallActivityAsync("FlakyFunction");
            Task timeoutTask = context.CreateTimer(deadline, cts.Token);

            Task winner = await Task.WhenAny(activityTask, timeoutTask);
            if (winner == activityTask)
            {
                // success case
                cts.Cancel();
                return true;
            }
            else
            {
                // timeout case
                return false;
            }
        }
    }
}
