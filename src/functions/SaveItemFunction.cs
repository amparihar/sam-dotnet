
using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Newtonsoft.Json;

using Lambda.Models;
using Lambda.Handlers;
using Item.Service;

namespace Lambda.Functions
{
    public class SaveItemFunction
    {
        private readonly IItemService _itemService;

        private readonly string _tableName = Environment.GetEnvironmentVariable("ITEM_TABLE_NAME");

        public SaveItemFunction()
        {
            _itemService = new ItemService();
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var requestModel = JsonConvert.DeserializeObject<SaveItemRequest>(request.Body);
            try
            {
                var result = await _itemService.SaveItem(requestModel);
                if (result != null)
                {
                    return ResponseHandler.ProcessResponse(HttpStatusCode.Created, JsonConvert.SerializeObject(result));
                }
                return ResponseHandler.ProcessResponse(HttpStatusCode.NotFound, $"Resource: {_tableName} not found");
            }
            catch (AmazonDynamoDBException ddbEx)
            {
                context.Logger.LogLine($"DynamoDB Error while saving Item: {requestModel}");
                context.Logger.LogLine($"DynamoDB Error: {ddbEx.StackTrace}");
                return ResponseHandler.ProcessResponse(HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(new ErrorBody
                {
                    Error = "DynamoDB Service Exception",
                    Message = ddbEx.Message
                }));
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Error while saving Item: {requestModel}");
                context.Logger.LogLine($"Error: {ex.StackTrace}");
                return ResponseHandler.ProcessResponse(HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(new ErrorBody
                {
                    Error = "Service Exception",
                    Message = ex.Message
                }));

            }
        }
    }
}
