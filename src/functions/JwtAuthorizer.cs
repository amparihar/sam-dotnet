using System;
using System.Linq;
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
            AuthPolicy authPolicy;
            AuthPolicyBuilder policyBuilder;
            try
            {
                context.Logger.LogLine($"{nameof(input.AuthorizationToken)}: {input.AuthorizationToken}");
                context.Logger.LogLine($"{nameof(input.MethodArn)}: {input.MethodArn}");

                var principalId = "";
                var tokenArr = input.AuthorizationToken?.Split(" ");
                var brearer = tokenArr.FirstOrDefault().ToLower();
                var token = tokenArr.LastOrDefault();

                if (brearer == "bearer" && !string.IsNullOrEmpty(token))
                {
                    principalId = JwtHandler.GetClaim(token);
                }

                if (!string.IsNullOrEmpty(principalId))
                {
                    policyBuilder = new AuthPolicyBuilder(principalId, null);
                    policyBuilder.AllowResources();
                }
                else
                {
                    policyBuilder = new AuthPolicyBuilder(principalId, null);
                    policyBuilder.DenyResources();
                }
                authPolicy = policyBuilder.Build();

                // additional context key-value pairs. "principalId" is implicitly passed in as a key-value pair
                // context values are  available by APIGW in : context.Authorizer.<key>
                //authPolicy.Context.Add("userName", "my-user-name");
                return authPolicy;
            }
            catch (Exception ex)
            {
                context.Logger.LogLine(ex.ToString());
                if (ex is UnauthorizedException)
                {
                    policyBuilder = new AuthPolicyBuilder("", null);
                    policyBuilder.DenyResources();
                    authPolicy = policyBuilder.Build();
                    authPolicy.Context.Add("message", ex.Message);
                    return authPolicy;
                    throw;
                }
                throw new UnauthorizedException();
            }
        }
    }
}