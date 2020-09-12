
using System;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Collections.Generic;
using Newtonsoft.Json;

using Cloud.AWS.DynamoDb;
using Lambda.Models;

namespace Lambda.Functions
{
    public class SaveItemFunction
    {
        private readonly DynamoDBService _dbService;

        public SaveItemFunction()
        {
            _dbService = new DynamoDBService();
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public APIGatewayProxyResponse Run(APIGatewayProxyRequest request)
        {
            var requestModel = JsonConvert.DeserializeObject<SaveItemRequest>(request.Body);
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(requestModel),
                Headers = new Dictionary<string, string>{
                  { "Content-Type", "application/json" },
                  { "Access-Control-Allow-Origin","*" }
              }
            };
        }
    }
}
