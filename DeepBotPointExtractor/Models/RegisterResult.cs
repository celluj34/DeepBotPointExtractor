using Newtonsoft.Json;

namespace DeepBotPointExtractor.Models
{
    public class RegisterResult : CommandResult
    {
        [JsonProperty(PropertyName = "msg")]
        public string Message {get;set;}
    }
}