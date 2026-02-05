namespace DotRabbit.Core.Connection;

public sealed class RmqHost
{
    public string Host { get; }
    public int Port { get; }

    public RmqHost(string host, int port)
    {
        Host = host;
        Port = port;
    }

    public static bool TryParse(
        string value,
        int defaultPort,
        out RmqHost? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.Trim();

        // IPv6: [::1]:5672
        if (value.StartsWith('['))
        {
            if (!Uri.TryCreate($"amqp://{value}", UriKind.Absolute, out var uri))
                return false;

            result = new RmqHost(
                uri.Host,
                uri.Port > 0 ? uri.Port : defaultPort);

            return result.Port > 0;
        }

        // host or host:port
        var parts = value.Split(':', 2);

        var host = parts[0];
        if (string.IsNullOrWhiteSpace(host))
            return false;

        var port = defaultPort;

        if (parts.Length == 2)
        {
            if (!int.TryParse(parts[1], out port))
                return false;
        }

        if (port <= 0 || port > 65535)
            return false;

        result = new RmqHost(host, port);
        return true;
    }
}
