
using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using Cloud.AWS.DynamoDb;

using Lambda.Models;
using Lambda.Handlers;
using Lambda.Mappers;

namespace Lambda.Functions
{
    public class GetItemsFunction
    {
        private DynamoDBService _dbService;

        private readonly string _tableName = Environment.GetEnvironmentVariable("ITEM_TABLE_NAME");

        public GetItemsFunction()
        {
            
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var requestModel = JsonConvert.DeserializeObject<GetItemRequest>(request.Body);
            try
            {
                _dbService = new DynamoDBService();
                using (var client = _dbService.DbClient)
                {
                    Table ItemTable;
                    var loadTableSuccess = false;
                    loadTableSuccess = Table.TryLoadTable(
                        client,
                        _tableName,
                        DynamoDBEntryConversion.V2, out ItemTable);
                    if (loadTableSuccess)
                    {
                        var filter = new QueryFilter("id", QueryOperator.Equal, requestModel.Id);
                        filter.AddCondition("key", QueryOperator.BeginsWith, requestModel.Key);

                        var documentResponse = await ItemTable.Query(filter).GetRemainingAsync();
                        // TO DO: Use Extention Method 
                        // Modify mapper functions as extension methods
                        var queryResponse = Mapper.ToItemResponse(documentResponse);

                        return ResponseHandler.ProcessResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(queryResponse));
                    }
                    return ResponseHandler.ProcessResponse(HttpStatusCode.NotFound, $"Resource: {_tableName} not found");
                }
            }
            catch (AmazonDynamoDBException ddbEx)
            {
                context.Logger.LogLine($"DynamoDB Error while Querying Item: {requestModel}");
                context.Logger.LogLine($"DynamoDB Error: {ddbEx.StackTrace}");
                return ResponseHandler.ProcessResponse(HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(new ErrorBody
                {
                    Error = "DynamoDB Service Exception",
                    Message = ddbEx.Message
                }));
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Error while Querying Item: {requestModel}");
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
