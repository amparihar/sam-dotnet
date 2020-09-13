
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
    public class SaveItemFunction
    {
        private DynamoDBService _dbService;

        private readonly string _tableName = Environment.GetEnvironmentVariable("ITEM_TABLE_NAME");

        public SaveItemFunction()
        {

        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var requestModel = JsonConvert.DeserializeObject<SaveItemRequest>(request.Body);
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
                        // TO DO: Use Extension method
                        var newItem = Mapper.ToSaveItemDocumentModel(requestModel);
                        await ItemTable.PutItemAsync(newItem);
                        return ResponseHandler.ProcessResponse(HttpStatusCode.Created, JsonConvert.SerializeObject(requestModel));
                    }
                    return ResponseHandler.ProcessResponse(HttpStatusCode.NotFound, $"Resource: {_tableName} not found");
                }
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
