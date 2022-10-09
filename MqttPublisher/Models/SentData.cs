using Newtonsoft.Json;
using System;

namespace MqttPublisher
{
    [Serializable]
    public class SentData
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
