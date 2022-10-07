using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace EventIngester.Functions.HttpTrigger
{
    public class Functionsapp
    {
        private readonly ILogger _logger;

        public Functionsapp(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Functionsapp>();
        }



        public class MyOutputType
        {
            [EventHubOutput("EHname", Connection = "connection")]
            public string Name { get; set; }

            public HttpResponseData HttpResponse { get; set; }
        }



        [Function(nameof(Functionsapp))]
        //[ExponentialBackoffRetry(2, "00:00:04", "00:15:00")]
        public static MyOutputType Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req, FunctionContext executionContext)

        {
            _logger.LogInformation($"First Event Hubs triggered message: xxxxxxxx");

            var exceptions = new List<Exception>();
            try
            {
                // Extract
                var Event = ExtractCloudEventFromRequest(req);


                // Guards and Validators

                var CorrelationCheck = ValidateCorrelationId(Event);

                var NameCheck1 = ValidateServiceBusNamingConvensionfromInput(Event.Subject);
                var NameCheck2 = ValidateServiceBusNamingConvensionfromInput(Event.Type);
                var NameCheck3 = ValidateServiceBusNamingConvensionfromInput(Event.Source);

                // Validate CloudEvent to Spec
                var EventValidatedToCloudEvent = ValidateCloudEventToSpecVersion(Event);

                // Validate Events data.
                var EventValidatedToDataModel = ValidateCloudEventDataModelToEventHubSchemaRegistry(Event);

            }
            catch (Exception ex)
            {

                // if ex > 0 
                //retutn bad thing in http code

                //  333
                throw;
                exceptions.Add(ex);
            }
            if (exceptions.Count > 1)
            {
                throw new AggregateException(exceptions);
            }

            // send happy 
            await outputCloudEvent.AddAsync(Event).ConfigureAwait(false);



            /// we should design this not as happy by defensive. 
            /// # Defensive Azure Functions best practices. 


            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return new MyOutputType()
            {
                Name = "some name",
                HttpResponse = response
            };
        }



        #region

        private static object ValidateServiceBusNamingConvensionfromInput(string input)
        {
            const string pattern = @"^[a-zA-Z0-9._/-]*$";
            var rgx = new Regex(pattern);
            var isMatch = rgx.IsMatch(input);
            if (!isMatch)
            {
                _logger.LogError($"Type {input} doesn't match regex {pattern}");
               
            }
            return isMatch;
        }

        private static object ValidateCloudEventDataModelToEventHubSchemaRegistry(CloudEvent Event)
        {
            throw new NotImplementedException();
        }

        private static CloudEvent ValidateCloudEventToSpecVersion(CloudEvent Event)
        {

            // check CloudEvent verison is = "1.0"
            throw new NotImplementedException();
        }

        private static string ValidateCorrelationId(CloudEvent Event)
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

        private static CloudEvent ExtractCloudEventFromRequest(HttpRequestData req)
        {
            var inputBinary = BinaryData.FromStream(req.Body);
            return CloudEvent.Parse(inputBinary);
        }
        #endregion















    }
}
