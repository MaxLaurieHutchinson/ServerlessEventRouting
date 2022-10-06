using Azure.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerlessEventOrchestration
{
    /// <summary>
    /// Cloud Event Spec-version "1.0" Http Ingester to Event Hubs 
    /// This Function will validate and then publish the events. 
    ///
    /// ID          // Identifies the event. Producers MUST ensure that source + id is unique for each distinct event. If a duplicate event is re-sent (e.g. due to a network error) it MAY have the same id
    /// Source      // Identifies the context in which an event happened. Often this will include information such as the type of the event source, the organization publishing the event or the process that produced the event
    /// Subject     // This describes the subject of the event in the context of the event producer (identified by source).
    /// Type        // This attribute contains a value describing the type of event related to the originating occurrence. Often this attribute is used for routing, observability, policy enforcement
    /// DataSchema? // URI  Identifies the schema that data adheres to // should be EH Schema Registry Model link URI?
    /// </summary>
    public static class HttpCloudEventOrchestrator
    {

        
        [FunctionName("HttpCloudEventOrchestrator")]
        public static async Task<IActionResult> Run(
            [EventHub("Name", Connection = "Connection")] IAsyncCollector<CloudEvent> outputCloudEvent,
            [HttpTrigger(AuthorizationLevel.Function,"post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Event received");


            var Event = ExtractCloudEventFromRequest(log, req);


            log.LogInformation("Event received {type} {subject}", Event.Type, Event.Subject);

            var CorrelationCheck = ValidateCorrelationId(log, Event);

            var NameCheck1 = ValidateServiceBusNamingConvensionfromInput(log, Event.Subject);
            var NameCheck2 = ValidateServiceBusNamingConvensionfromInput(log, Event.Type);
            var NameCheck3 = ValidateServiceBusNamingConvensionfromInput(log, Event.Source);

            // Validate CloudEvent to Spec
            var EventValidatedToCloudEvent = ValidateCloudEventToSpecVersion(log, Event);

            // Validate Events data.
             var EventValidatedToDataModel = ValidateCloudEventDataModelToEventHubSchemaRegistry(log, Event);

            // Publish Events onto Event Hub.

            // Testing stuff
            string responseMessage = "got it";

            log.LogInformation("Event processed {type} {subject}", Event.Type, Event.Subject);

            // TODO: outputting better results if not 200
            return new OkObjectResult(responseMessage);
        }

        private static object ValidateServiceBusNamingConvensionfromInput(ILogger log, string input)
        {
            const string pattern = @"^[a-zA-Z0-9._/-]*$";
            var rgx = new Regex(pattern);
            var isMatch = rgx.IsMatch(input);
            if (!isMatch)
            {
                log.LogError($"Type {input} doesn't match regex {pattern}");
                return new BadRequestObjectResult("Type must only contain letters, numbers, and dashes");
            }
            return isMatch;
        }

        private static object ValidateCloudEventDataModelToEventHubSchemaRegistry(ILogger log, CloudEvent Event)
        {
            throw new NotImplementedException();
        }

        private static CloudEvent ValidateCloudEventToSpecVersion(ILogger log, CloudEvent Event)
        {

            // check CloudEvent verison is = "1.0"
            throw new NotImplementedException();
        }

        private static string ValidateCorrelationId(ILogger log, CloudEvent Event)
        {
            // Check value in the CloudEvent
            if (!Event.ExtensionAttributes.TryGetValue("correlationid", out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }
            // log for Analytics 
            log.LogInformation($"CorrelationID: {correlationId}, MessageID: {Event.Id}");

            return correlationId.ToString();
        }

        private static CloudEvent ExtractCloudEventFromRequest(ILogger log, HttpRequest req)
        {
            var inputBinary = BinaryData.FromStream(req.Body);
            return CloudEvent.Parse(inputBinary);
        }
    }
}