// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;
using Azure.Messaging;

namespace ServerlessEventOrchestration
{
    public static class EventGridCloudEventOrchestrator
    {
        [FunctionName("EventGridCloudEventOrchestrator")]
        public static void Run([EventGridTrigger]CloudEvent Event, ILogger log)
        {
            log.LogInformation("Event received {type} {subject}", Event.Type, Event.Subject);
        }
    }
}
