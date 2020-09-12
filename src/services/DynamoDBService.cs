using System;
using Amazon.DynamoDBv2;

using Amazon.DynamoDBv2.DataModel;

namespace Cloud.AWS.DynamoDb
{
    public class DynamoDBService
    {
        public readonly AmazonDynamoDBClient DbClient;

        public DynamoDBService()
        {
            AmazonDynamoDBConfig DynamoConfig = new AmazonDynamoDBConfig();
            DynamoConfig.ServiceURL = Environment.GetEnvironmentVariable("DYNAMODB_SERVICEURL");
            DbClient = new AmazonDynamoDBClient(DynamoConfig);
        }
    }
}