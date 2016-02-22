using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeepBotPointExtractor.Models
{
    public class UserResult : CommandResult
    {
        [JsonProperty(PropertyName = "msg")]
        public List<User> Message {get;set;}
    }
}