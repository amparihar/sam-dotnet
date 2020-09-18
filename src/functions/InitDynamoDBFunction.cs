
using System;
using System.Net;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

using Lambda.Models;
using Lambda.Handlers;
using Item.Service;

namespace Lambda.Functions
{
    // https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/crpg-ref-requesttypes.html
    public class CfnRequest
    {
        public string RequestType { get; set; }
        public string ResponseURL { get; set; }
        public string StackId { get; set; }
        public string RequestId { get; set; }
        public string ResourceType { get; set; }
        public string LogicalResourceId { get; set; }
        public ResourceProperties ResourceProperties { get; set; }
        public ResourceProperties OldResourceProperties { get; set; }
    }

    public class ResourceProperties
    {
        public string ServiceToken { get; set; }
        public string Region { get; set; }
        public string DatabaseName { get; set; }
        public string Engine { get; set; }
        public string GlobalClusterIdentifier { get; set; }
    }

    // Reference
    // https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/crpg-ref-responses.html
    public class CfnResponse
    {
        public string Status { get; set; }
        public string Reason { get; set; }
        public string PhysicalResourceId { get; set; }
        public string StackId { get; set; }
        public string RequestId { get; set; }
        public string LogicalResourceId { get; set; }
        public bool NoEcho { get; set; }
    }

    public enum CfnResponseStatus
    {
        SUCCESS = 0,
        FAILED = 1
    }
    public class InitDynamoDBFunction
    {
        private readonly IItemService _itemService;

        private readonly string _tableName = Environment.GetEnvironmentVariable("ITEM_TABLE_NAME");

        public InitDynamoDBFunction()
        {
            _itemService = new ItemService();
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task Run(object cfnRequest, ILambdaContext context)
        {
            LogHandler.LogMessage(context, cfnRequest.ToString());
            var request = JsonConvert.DeserializeObject<CfnRequest>(cfnRequest.ToString());

            var response = new CfnResponse
            {
                StackId = request.StackId,
                RequestId = request.RequestId,
                LogicalResourceId = request.LogicalResourceId,
                PhysicalResourceId = $"{context.AwsRequestId}.{request.LogicalResourceId}",
                Reason = string.Empty, // Mandatory if status is FAILED
                NoEcho = false
            };
            switch (request.RequestType)
            {
                case "Create":
                    await CreateSeed(request, response, context);
                    break;
                default:
                    response.Status = "SUCCESS";
                    break;
            }
            SendResponse(request, response);
        }

        async Task CreateSeed(CfnRequest cfnRequest, CfnResponse cfnResponse, ILambdaContext context)
        {
            try
            {
                var result = await _itemService.SaveItem(new SaveItemRequest
                {
                    Id = "U1",
                    Name = "This is a custom data inserted using a custom resource",
                    Key = "Seed Data Key"
                });
                if (result != null)
                {
                    // TO DO : use immer. do not mutate!!!
                    cfnResponse.Status = "SUCCESS";
                    LogHandler.LogMessage(context, "Data Seeded Successfully");

                }
                else
                {
                    // TO DO : use immer. do not mutate!!!
                    cfnResponse.Status = "FAILED";
                    cfnResponse.Reason = $"Resource: {_tableName} not found";
                    LogHandler.LogMessage(context, "Data Seed Failed");
                }
            }
            catch (Exception ex)
            {
                // TO DO : use immer. do not mutate!!!
                cfnResponse.Status = "FAILED";
                cfnResponse.Reason = ex.Message;
                LogHandler.LogMessage(context, ex.StackTrace);
            }
        }

        // Explicit call to cfn ResponseURL to confirm creation of custom resource
        void SendResponse(CfnRequest cfnRequest, CfnResponse cfnResponse)
        {
            string json = JsonConvert.SerializeObject(cfnResponse);
            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            var httpRequest = WebRequest.Create(cfnRequest.ResponseURL) as HttpWebRequest;
            httpRequest.Method = "PUT";
            httpRequest.ContentType = "";
            httpRequest.ContentLength = byteArray.Length;

            using (Stream datastream = httpRequest.GetRequestStream())
            {
                datastream.Write(byteArray, 0, byteArray.Length);
            }
            httpRequest.GetResponse();
        }
    }
}