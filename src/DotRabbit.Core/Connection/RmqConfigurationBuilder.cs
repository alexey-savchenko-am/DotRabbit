namespace DotRabbit.Core.Connection;

public class RmqConfigurationBuilder
{
    private readonly List<RmqHost> _hosts = new();
    private Scheme _scheme = Scheme.Amqp;
    private string? _userName;
    private string? _password;
    private string _virtualHost = "/";
    private bool _automaticRecoveryEnabled = true;
    private bool _built;

    private RmqConfigurationBuilder() { }

    public static RmqConfigurationBuilder Create()
           => new();

    public RmqConfigurationBuilder WithScheme(Scheme scheme)
    {
        _scheme = scheme;
        return this;
    }

    public RmqConfigurationBuilder WithCredentials(
        string? userName,
        string? password)
    {
        _userName = userName;
        _password = password;
        return this;
    }

    public RmqConfigurationBuilder WithVirtualHost(string virtualHost)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(virtualHost);
        _virtualHost = virtualHost;
        return this;
    }

    public RmqConfigurationBuilder AddHost(string host, int port)
    {
        _hosts.Add(new RmqHost(host, port));
        return this;
    }

    public RmqConfigurationBuilder WithHosts(IEnumerable<RmqHost> hosts)
    {
        _hosts.Clear();
        _hosts.AddRange(hosts);
        return this;
    }

    public RmqConfigurationBuilder WithAutomaticRecovery(bool enabled)
    {
        _automaticRecoveryEnabled = enabled;
        return this;
    }

    public static RmqConnectionConfiguration FromConnectionString(
        string connectionString,
        bool automaticRecoveryEnabled = true)
    {
        var config = RmqConnectionConfiguration
            .FromConnectionString(connectionString, automaticRecoveryEnabled);

        return config;
    }

    public RmqConnectionConfiguration Build()
    {
        if (_built)
            throw new InvalidOperationException("Builder already used");

        if (_hosts.Count == 0)
            throw new InvalidOperationException(
                "At least one RMQ host must be specified");

        _built = true;

        return new RmqConnectionConfiguration
        {
            Scheme = _scheme,
            UserName = _userName,
            Password = _password,
            VirtualHost = _virtualHost,
            Hosts = _hosts.AsReadOnly(),
            AutomaticRecoveryEnabled = _automaticRecoveryEnabled
        };
    }

}
