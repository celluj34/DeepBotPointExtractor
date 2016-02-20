using System;
using Newtonsoft.Json;

namespace DeepBotPointFucker.Models
{
    public class User
    {
        [JsonProperty(PropertyName = "user")]
        public string Name {get;set;}

        [JsonProperty(PropertyName = "points")]
        public decimal Points {get;set;}

        [JsonProperty(PropertyName = "watch_time")]
        public decimal WatchTime {get;set;}

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
    }
}