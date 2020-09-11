
using System;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lambda.Functions
{
    public class GetItemsFunction
    {
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(request),
                Headers = new Dictionary<string, string>{
                  { "Content-Type", "application/json" },
                  { "Access-Control-Allow-Origin","*" }
              }
            };
        }
    }
}
