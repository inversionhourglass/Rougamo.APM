using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Rougamo.APM.Serialization
{
    class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

        public const string OPTIONS_NAME = "rougamo.apm";

        public JsonSerializer(IOptionsMonitor<JsonSerializerSettings> options)
        {
            _settings = options.Get(OPTIONS_NAME);
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _settings);
        }
    }
}
