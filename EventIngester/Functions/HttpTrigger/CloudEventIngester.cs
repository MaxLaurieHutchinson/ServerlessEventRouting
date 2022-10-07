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
            public CloudEvent Event { get; set; }

            public HttpResponseData HttpResponse { get; set; }
        }

        [Function(nameof(Functionsapp))]
        public MyOutputType Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req, ILogger l)

        {
            _logger.LogInformation($"First Event Hubs triggered message: xxxxxxxx");

            var exceptions = new List<Exception>();
            CloudEvent CEvent;
            try
            {
                // Extract
                CEvent = ExtractCloudEventFromRequest(req);

                // Guards and Validators

                var CorrelationCheck = ValidateCorrelationId(CEvent);

                var NameCheck1 = ValidateServiceBusNamingConvensionfromInput(CEvent.Subject);
                var NameCheck2 = ValidateServiceBusNamingConvensionfromInput(CEvent.Type);
                var NameCheck3 = ValidateServiceBusNamingConvensionfromInput(CEvent.Source);

                // Validate CloudEvent to Spec
                var EventValidatedToCloudEvent = ValidateCloudEventToSpecVersion(CEvent);

                // Validate Events data.
                var EventValidatedToDataModel = ValidateCloudEventDataModelToEventHubSchemaRegistry(CEvent);
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
            // old binding in process? 
            //await MyOutputType.AddAsync(Event).ConfigureAwait(false);

            /// we should design this not as happy by defensive.
            /// # Defensive Azure Functions best practices.

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return new MyOutputType()
            {
                Event = CEvent,
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
                // throw exception // Type {input} doesn't match regex {pattern}
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
            // log for Analytics log.LogInformation($"CorrelationID: {correlationId}, MessageID: {Event.Id}");

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