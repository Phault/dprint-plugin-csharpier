using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Dprint.Plugins.Csharpier.Serialization;

public class JsonSerializer
{
    public T Deserialize<T>(byte[] jsonData)
    {
        var jsonText = Encoding.UTF8.GetString(jsonData);
        return JsonConvert.DeserializeObject<T>(jsonText, GetSettings())
            ?? throw new Exception("Error deserializing JSON.");
    }

    public byte[] Serialize<T>(T obj)
    {
        var jsonText =
            JsonConvert.SerializeObject(obj, GetSettings())
            ?? throw new Exception("Error serializing to JSON.");
        return Encoding.UTF8.GetBytes(jsonText);
    }

    private JsonSerializerSettings GetSettings()
    {
        return new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            },
            Converters = [new StringEnumConverter(new CamelCaseNamingStrategy())]
        };
    }
}
