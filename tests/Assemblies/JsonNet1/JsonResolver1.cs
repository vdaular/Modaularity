using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TestInterfaces;

namespace JsonNet1;

public class JsonResolver1 : IJsonVersionResolver
{
    public string GetVersion()
    {
        var result = typeof(JsonConvert).Assembly.GetName().Version.ToString();

        return result;
    }

    public string GetLoggingVersion()
    {
        var logging = new LoggerFactory();
        Console.WriteLine(logging.ToString());

        var result = typeof(LoggerFactory).Assembly.GetName().Version.ToString();

        return result;
    }
}
