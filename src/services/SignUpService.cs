using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

using Cloud.AWS.DynamoDb;
using Lambda.Mappers;
using Lambda.Models;

namespace SignUp.Service
{

    public class SignUpService
    {
        private readonly string _tableName = Environment.GetEnvironmentVariable("USER_TABLE_NAME");
        private readonly Table _table;
        private bool _loadTableSuccess { get; set; }
        private readonly DynamoDBService _dbService;
        private readonly UserMapper _map = new UserMapper();
        public SignUpService()
        {
            _dbService = new DynamoDBService();
            _loadTableSuccess = Table.TryLoadTable(
                _dbService.DbClient,
                _tableName,
                DynamoDBEntryConversion.V2, out _table);
        }

        public async Task<SignUpRequest> SignUp(SignUpRequest request)
        {
            if (_loadTableSuccess)
            {
                var newItem = _map.ToSignUpDocumentModel(request);
                await _table.PutItemAsync(newItem);
                return request;
            }
            return null;
        }
    }


}