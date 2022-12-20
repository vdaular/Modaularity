using Newtonsoft.Json;
using TestInterfaces;

namespace JsonNet2;

public class JsonResolver2 : IJsonVersionResolver
{
    public string GetVersion()
    {
        var result = typeof(JsonConvert).Assembly.GetName().Version.ToString();

        return result;
    }
}
