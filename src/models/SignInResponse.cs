
using Newtonsoft.Json;

namespace Lambda.Models
{
    public class SignInResponse
    {
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; } = "";

        [JsonProperty(PropertyName = "accessToken")]
        public string Token { get; set; } = "";

        [JsonIgnore]
        public bool Authenticated { get; set; } = false;

        public SignInResponse() : this("", "")
        {

        }

        public SignInResponse(string userName, string token)
        {
            Authenticated = string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(token) ? false : true;
            UserName = userName;
            Token = token;
        }
    }
}