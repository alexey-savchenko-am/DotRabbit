namespace DotRabbit.Core.Connection;

public sealed class RmqConnectionConfiguration
{
    public Scheme Scheme { get; init; }
    public string? UserName { get; init; }
    public string? Password { get; init; }
    public string VirtualHost { get; init; } = "/";
    public IReadOnlyList<RmqHost> Hosts { get; init; } = new List<RmqHost>();
    public bool AutomaticRecoveryEnabled { get; init; } = true;

    // ex: amqp://guest:guest@localhost:5671/localhost
    public static RmqConnectionConfiguration FromConnectionString(
        string connectionString,
        bool automaticRecoveryEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is empty", nameof(connectionString));

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid RMQ connection string", nameof(connectionString));

        if (uri.Scheme != "amqp" && uri.Scheme != "amqps")
            throw new ArgumentException($"Unsupported scheme '{uri.Scheme}'");

        var scheme = uri.Scheme == "amqps" ? Scheme.Amqps : Scheme.Amqp;

        var (user, pass) = ParseUserInfo(uri.UserInfo);

        var vhost = string.IsNullOrEmpty(uri.AbsolutePath.Trim('/'))
            ? "/"
            : Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));

        var port = uri.Port > 0
            ? uri.Port
            : scheme == Scheme.Amqps ? 5671 : 5672;

        return new RmqConnectionConfiguration
        {
            Scheme = scheme,
            UserName = user,
            Password = pass,
            VirtualHost = vhost,
            Hosts = 
            [
                new RmqHost(uri.Host, port)
            ],
            AutomaticRecoveryEnabled = automaticRecoveryEnabled
        };
    }

    private static (string? user, string? pass) ParseUserInfo(string userInfo)
    {
        if (string.IsNullOrEmpty(userInfo))
            return (null, null);

        var parts = userInfo.Split(':', 2);
        return (
            Uri.UnescapeDataString(parts[0]),
            parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : null
        );
    }
}

public enum Scheme
{
    Amqp = 0,
    Amqps = 1,
}
