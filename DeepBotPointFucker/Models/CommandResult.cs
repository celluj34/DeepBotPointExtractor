using Newtonsoft.Json;

namespace DeepBotPointFucker.Models
{
    public class CommandResult
    {
        [JsonProperty(PropertyName = "function")]
        public string Function {get;set;}

        [JsonProperty(PropertyName = "param")]
        public string Parameter {get;set;}
    }
}