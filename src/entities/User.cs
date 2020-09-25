
using Newtonsoft.Json;

namespace Lambda.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        [JsonIgnore]
        public string Password { get; set; }
    }
}