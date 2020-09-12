
using System;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Collections.Generic;
using Newtonsoft.Json;

using Cloud.AWS.DynamoDb;

namespace Lambda.Functions
{
    public class GetItemsFunction
    {
        private readonly DynamoDBService _dbService;

        public GetItemsFunction()
        {
            _dbService = new DynamoDBService();
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public APIGatewayProxyResponse Run(APIGatewayProxyRequest request)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(_dbService.DbClient),
                Headers = new Dictionary<string, string>{
                  { "Content-Type", "application/json" },
                  { "Access-Control-Allow-Origin","*" }
              }
            };
        }
    }
}
