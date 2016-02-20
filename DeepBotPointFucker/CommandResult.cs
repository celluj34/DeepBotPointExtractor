using Newtonsoft.Json;

namespace DeepBotPointFucker
{
    public class CommandResult
    {
        /*
        [JsonProperty(PropertyName = "function")]
        public string Function {get;set;}

        [JsonProperty(PropertyName = "param")]
        public string Parameter {get;set;}
        */

        [JsonProperty(PropertyName = "msg")]
        public string Message {get;set;}
    }
}