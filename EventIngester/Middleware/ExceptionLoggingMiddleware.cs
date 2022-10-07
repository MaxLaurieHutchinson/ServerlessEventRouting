using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace Ingester.Middleware;

public class ExceptionLoggingMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            var logger = context.GetLogger(context.FunctionDefinition.Name);
            logger.LogInformation($"context.FunctionDefinition.Name Started processing a message: {0}: {1}", context.FunctionDefinition.Name, context.InvocationId.ToString());
            await next(context);
            logger.LogInformation($"context.FunctionDefinition.Name Finished processing a message: {0}: {1}", context.FunctionDefinition.Name, context.InvocationId);
        }
        catch (Exception ex)
        {
            if (ex is AggregateException aggregateException)
            {
                /// <remarks>
                /// The first error from the InnerExceptions collection.
                /// This will slim the Exception message considerably.
                /// </remarks>
                ex = aggregateException.InnerExceptions.First();
            }

            var logger = context.GetLogger(context.FunctionDefinition.Name);
            logger.LogError("Unexpected Error in {0}: {1}", context.FunctionDefinition.Name, ex.Message);


        }
    }
}