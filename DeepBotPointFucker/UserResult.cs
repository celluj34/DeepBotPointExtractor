using Newtonsoft.Json;

namespace DeepBotPointFucker
{
    public class UserResult
    {
        [JsonProperty(PropertyName = "user")]
        public string User {get;set;}

        [JsonProperty(PropertyName = "points")]
        public decimal Points {get;set;}

        /*
        [JsonProperty(PropertyName = "watch_time")]
        public int WatchTime {get;set;}

        [JsonProperty(PropertyName = "vip")]
        public int Vip {get;set;}

        [JsonProperty(PropertyName = "mod")]
        public int Mod {get;set;}

        [JsonProperty(PropertyName = "join_date")]
        public DateTime JoinDate {get;set;}

        [JsonProperty(PropertyName = "last_seen")]
        public DateTime LastSeen {get;set;}

        [JsonProperty(PropertyName = "vip_expiry")]
        public DateTime VipExpiry {get;set;}
        */
    }
}