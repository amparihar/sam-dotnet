
using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Newtonsoft.Json;

using Lambda.Models;
using Lambda.Handlers;
using User.Service;

namespace Lambda.Functions
{
    public class SignUpFunction
    {
        private readonly IUserService _userService;

        private readonly string _tableName = Environment.GetEnvironmentVariable("USER_TABLE_NAME");

        public SignUpFunction()
        {
            _userService = new UserService();
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var requestModel = JsonConvert.DeserializeObject<SignUpRequest>(request.Body);
            try
            {
                var result = await _userService.SignUp(requestModel);
                if (result != null)
                {
                    return ResponseHandler.ProcessResponse(HttpStatusCode.Created, JsonConvert.SerializeObject(result));
                }
                return ResponseHandler.ProcessResponse(HttpStatusCode.NotFound, $"Resource: {_tableName} not found");
            }
            catch (AmazonDynamoDBException ddbEx)
            {
                LogHandler.LogMessage(context, $"DynamoDB Error while SignUp: {requestModel}");
                LogHandler.LogMessage(context, $"DynamoDB Error: {ddbEx.StackTrace}");

                return ResponseHandler.ProcessResponse(HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(new ErrorBody
                {
                    Error = "DynamoDB Service Exception",
                    Message = ddbEx.Message
                }));
            }
            catch (Exception ex)
            {
                LogHandler.LogMessage(context, $"Error while SignUp: {requestModel}");
                LogHandler.LogMessage(context, $"Error: {ex.StackTrace}");

                return ResponseHandler.ProcessResponse(HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(new ErrorBody
                {
                    Error = "Service Exception",
                    Message = ex.Message
                }));
            }
        }
    }
}
