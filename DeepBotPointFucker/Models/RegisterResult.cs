using Newtonsoft.Json;

namespace DeepBotPointFucker.Models
{
    public class RegisterResult : CommandResult
    {
        [JsonProperty(PropertyName = "msg")]
        public string Message {get;set;}
    }
}