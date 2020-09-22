
using System;
using Amazon.Lambda.Core;

using APIGateway.Auth;
using APIGateway.Auth.Model;

namespace Lambda.Functions
{
    public class JwtAuthorizer
    {
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public AuthPolicy Authorize(TokenAuthorizerContext input, ILambdaContext context)
        {
            try
            {
                context.Logger.LogLine($"{nameof(input.AuthorizationToken)}: {input.AuthorizationToken}");
                context.Logger.LogLine($"{nameof(input.MethodArn)}: {input.MethodArn}");

                var principalId = "";
                AuthPolicyBuilder policyBuilder;

                if (bool.Parse(input.AuthorizationToken))
                {
                    principalId = "user|u1";
                    policyBuilder = new AuthPolicyBuilder(principalId, null);
                    policyBuilder.AllowResources();
                }
                else
                {
                    policyBuilder = new AuthPolicyBuilder(principalId, null);
                    policyBuilder.DenyResources();
                }
                var authResponse = policyBuilder.Build();

                // additional context key-value pairs. "principalId" is implicitly passed in as a key-value pair
                // context values are  available by APIGW in : context.Authorizer.<key>
                authResponse.Context.Add("userName","my-user-name");
                return authResponse;
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedException)
                    throw;
                context.Logger.LogLine(ex.ToString());
                throw new UnauthorizedException();
            }
        }
    }

}