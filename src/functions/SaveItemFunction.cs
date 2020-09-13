
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
        private readonly DynamoDBService _dbService;

        public SaveItemFunction()
        {
            _dbService = new DynamoDBService();
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var requestModel = JsonConvert.DeserializeObject<SaveItemRequest>(request.Body);
            var TableName = Environment.GetEnvironmentVariable("ITEM_TABLE_NAME");
            try
            {
                using (var client = _dbService.DbClient)
                {
                    Table ItemTable;
                    var loadTableSuccess = false;
                    loadTableSuccess = Table.TryLoadTable(
                        client,
                        TableName,
                        DynamoDBEntryConversion.V2, out ItemTable);
                    if (loadTableSuccess)
                    {
                        var newItem = Mapper.ToSaveItemDocumentModel(requestModel);
                        await ItemTable.PutItemAsync(newItem);
                        return ResponseHandler.ProcessResponse(HttpStatusCode.Created, JsonConvert.SerializeObject(requestModel));
                    }
                    return ResponseHandler.ProcessResponse(HttpStatusCode.NotFound, $"Resource: {TableName} not found");
                }
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
