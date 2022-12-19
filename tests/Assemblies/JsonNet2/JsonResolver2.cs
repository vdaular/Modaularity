using System.Text.Json;
using TestInterfaces;

namespace JsonNet2;

public class JsonResolver2 : IJsonVersionResolver
{
    public string GetVersion()
    {
        var result = typeof(JsonSerializer).Assembly.GetName().Version.ToString();

        return result;
    }
}
