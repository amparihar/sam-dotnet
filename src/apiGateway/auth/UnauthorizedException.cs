namespace APIGateway.Auth
{
    public class UnauthorizedException : System.Exception
    {
        public UnauthorizedException() : base("Unauthorized")
        {
        }

        public UnauthorizedException(string message) : base(message)
        {

        }
    }
}
