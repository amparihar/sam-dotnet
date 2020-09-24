
using System;
using System.Net;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using ImmutableNet;
using ImmutableDotNet.Serialization.Newtonsoft;

using Lambda.Models;
using Lambda.Handlers;
using Item.Service;
using User.Service;

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
        public IEnumerable<string> SeedData { get; set; }
    }

    // Reference
    // https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/crpg-ref-responses.html
    public class CfnResponse
    {
        public string Status { get; set; } = "SUCCESS";
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
        private readonly IUserService _userService;

        private readonly string _tableName = Environment.GetEnvironmentVariable("ITEM_TABLE_NAME");

        public InitDynamoDBFunction()
        {
            _itemService = new ItemService();
            _userService = new UserService();
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task Run(object cfnRequest, ILambdaContext context)
        {
            var request = JsonConvert.DeserializeObject<CfnRequest>(cfnRequest.ToString());

            LogHandler.LogMessage(context, JsonConvert.SerializeObject(request));

            var response = Immutable.Create(new CfnResponse
            {
                StackId = request.StackId,
                RequestId = request.RequestId,
                LogicalResourceId = request.LogicalResourceId,
                PhysicalResourceId = $"{context.AwsRequestId}.{request.LogicalResourceId}",
                Reason = string.Empty, // Mandatory if status is FAILED
                NoEcho = false
            });
            Immutable<CfnResponse> cfnResponse;
            switch (request.RequestType)
            {
                case "Create":
                    cfnResponse = await CreateSeed(request, response, context);
                    break;
                case "Delete":
                    // should be used when table DeletionPolicy is Retain
                    cfnResponse = await DeleteTable(request, response, context);
                    break;
                default:
                    cfnResponse = response.Modify(r => r.Status = "SUCCESS");
                    break;
            }
            SendResponseToCfn(request, cfnResponse);
        }

        async Task<Immutable<CfnResponse>> CreateSeed(CfnRequest cfnRequest, Immutable<CfnResponse> cfnResponse, ILambdaContext context)
        {
            try
            {
                var seedData = cfnRequest.ResourceProperties.SeedData;
                if (seedData == null ||
                    seedData.Count() == 0 ||
                    (seedData.Count() == 1 && string.IsNullOrEmpty(seedData.First())))
                    return cfnResponse;

                var request = seedData.Select(seed =>
                {
                    return new SaveItemRequest
                    {
                        Id = "U1",
                        Name = "This is a custom data inserted using a custom resource",
                        Key = seed
                    };
                });

                var result = await _itemService.BatchWrite(request);
                if (result != null)
                {
                    LogHandler.LogMessage(context, "Db seeding successful");
                    return cfnResponse.Modify(r => r.Status = "SUCCESS");
                }
                else
                {
                    LogHandler.LogMessage(context, "Db seeding failed");
                    return cfnResponse.ToBuilder()
                        .Modify(r => r.Status = "FAILED")
                        .Modify(r => r.Reason = $"Resource: {_tableName} not found")
                        .ToImmutable();
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogMessage(context, ex.StackTrace);
                return cfnResponse.ToBuilder()
                        .Modify(r => r.Status = "FAILED")
                        .Modify(r => r.Reason = ex.Message)
                        .ToImmutable();
            }
        }

        async Task<Immutable<CfnResponse>> DeleteTable(CfnRequest cfnRequest, Immutable<CfnResponse> cfnResponse, ILambdaContext context)
        {
            try
            {
                await _itemService.DeleteTable();
                await _userService.DeleteTable();
                LogHandler.LogMessage(context, $"Request {cfnRequest.RequestType} executed Successfully");
                return cfnResponse.Modify(r => r.Status = "SUCCESS");
            }
            catch (Exception ex)
            {
                LogHandler.LogMessage(context, $"Message: {ex.Message} /Trace: {ex.StackTrace}");
                return cfnResponse.ToBuilder()
                        .Modify(r => r.Status = "FAILED")
                        .Modify(r => r.Reason = ex.Message)
                        .ToImmutable();
            }
        }

        // Explicit call to cfn ResponseURL to confirm creation of custom resource
        void SendResponseToCfn(CfnRequest cfnRequest, Immutable<CfnResponse> cfnResponse)
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new ImmutableJsonConverter());

            string json = JsonConvert.SerializeObject(cfnResponse, serializerSettings);

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
