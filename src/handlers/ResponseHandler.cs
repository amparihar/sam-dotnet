using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

namespace Lambda.Handlers
{
    public class ResponseHandler
    {
        public static APIGatewayProxyResponse ProcessResponse(HttpStatusCode code, string body)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)code,
                Body = body ?? string.Empty,
                Headers = new Dictionary<string, string>{
                        { "Content-Type", "application/json" },
                        { "Access-Control-Allow-Origin","*" }
                    }
            };
        }

        public APIGatewayProxyResponse ProcessResponse(HttpStatusCode code, IDictionary<string, string> body)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)code,
                Body = (body != null) ? JsonConvert.SerializeObject(body) : string.Empty,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" }
                }
            };
        }
    }
}