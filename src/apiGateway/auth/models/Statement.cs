using Newtonsoft.Json;

namespace APIGateway.Auth.Model
{
    public class Statement
    {
        [JsonProperty(PropertyName = "Action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "Effect")]
        public string Effect { get; set; } = "Deny"; // Default to Deny 

        [JsonProperty(PropertyName = "Resource")]
        public string Resource { get; set; }
        
    }
}