using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;

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
                ["password"] = BCrypt.Net.BCrypt.HashPassword(request.Password, BCrypt.Net.BCrypt.GenerateSalt(10))
            };
        }
    }
}