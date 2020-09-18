using Amazon.Lambda.Core;

namespace Lambda.Handlers
{
    public class LogHandler
    {
        public static void LogMessage(ILambdaContext ctx, string msg)
        {
            ctx.Logger.LogLine($"{ctx.AwsRequestId}:{ctx.FunctionName} - {msg}");
        }
    }
}