using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;

namespace Lambda.Handlers
{
    public class ResponseHandler
    {
        public static APIGatewayProxyResponse ProcessResponse(HttpStatusCode code, string body)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)code,
                Body = body ?? null,
                Headers = new Dictionary<string, string>{
                        { "Content-Type", "application/json" },
                        { "Access-Control-Allow-Origin","*" }
                    }
            };
        }
    }
}