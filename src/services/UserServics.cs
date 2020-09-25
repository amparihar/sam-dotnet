using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Cloud.AWS.DynamoDb;
using DynamoDBv2Model = Amazon.DynamoDBv2.Model;
using BCryptNet = BCrypt.Net;

using Lambda.Mappers;
using Lambda.Models;
using APIGateway.Auth;

namespace User.Service
{

    public class UserService : IUserService
    {
        private readonly string _tableName = Environment.GetEnvironmentVariable("USER_TABLE_NAME");
        private readonly Table _table;
        private bool _loadTableSuccess { get; set; }
        private readonly DynamoDBService _dbService;
        private readonly UserMapper _map = new UserMapper();
        public UserService()
        {
            _dbService = new DynamoDBService();
            _loadTableSuccess = Table.TryLoadTable(
                _dbService.DbClient,
                _tableName,
                DynamoDBEntryConversion.V2, out _table);
        }

        public async Task<SignInResponse> SignIn(SignInRequest request)
        {
            if (_loadTableSuccess)
            {
                var user = await _table.GetItemAsync(request.UserName);
                if (user != null)
                {
                    if (BCryptNet.BCrypt.Verify(request.Password, user["password"]))
                    {
                        var token = generateJwtToken(user);
                        return new SignInResponse(user["username"], token);
                    }
                    // Invalid password
                    return new SignInResponse();
                }
                // User not found
                return new SignInResponse();
            }
            return null;
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

        public async Task DeleteTable()
        {
            var request = new DynamoDBv2Model.DeleteTableRequest
            {
                TableName = _tableName
            };
            await _dbService.DbClient.DeleteTableAsync(request);
        }

        string generateJwtToken(Document user)
        {
            return JwtHandler.GenerateToken(user["id"].ToString());
        }
    }

}