using System;
using Amazon.DynamoDBv2.DocumentModel;
using BCryptNet = BCrypt.Net;

using Lambda.Models;

namespace Lambda.Mappers
{
    public class UserMapper
    {
        public Document ToSignUpDocumentModel(SignUpRequest request)
        {
            return new Document
            {
                ["id"] = Guid.NewGuid(),
                ["username"] = request.UserName,
                ["password"] = BCryptNet.BCrypt.HashPassword(request.Password, BCryptNet.BCrypt.GenerateSalt(10))
            };
        }
    }
}