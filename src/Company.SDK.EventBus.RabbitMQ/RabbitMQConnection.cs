using Company.SDK.EventBus.Abstractions.Events;
using RabbitMQ.Client;

namespace Company.SDK.EventBus.RabbitMQ;

public sealed class RabbitMQConnection : IRabbitMQConnection, IDisposable
{
    private readonly RabbitMQConfiguration _configuration;
    private ConnectionFactory _factory;
    private Dictionary<string, IConnection> _connections = new();

    public RabbitMQConnection(RabbitMQConfiguration config)
    {
        _configuration = config ?? throw new ArgumentNullException(nameof(config));

        if (string.IsNullOrEmpty(_configuration.Hostname))
            throw new ArgumentNullException(nameof(_configuration.Hostname));

        if (string.IsNullOrEmpty(_configuration.Username))
            throw new ArgumentNullException(nameof(_configuration.Username));

        if (string.IsNullOrEmpty(_configuration.Password))
            throw new ArgumentNullException(nameof(_configuration.Password));


        _factory = !string.IsNullOrEmpty(_configuration.Uri) ?
            new ConnectionFactory()
            {
                Uri = new Uri(_configuration.Uri),
                AutomaticRecoveryEnabled = _configuration.AutomaticRecoveryEnabled,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(_configuration.NetworkRecoveryIntervalSeconds)

            } :
            new ConnectionFactory()
            {
                HostName = _configuration.Hostname,
                Port = _configuration.Port,
                UserName = _configuration.Username,
                Password = _configuration.Password,
                VirtualHost = _configuration.VirtualHost,
                AutomaticRecoveryEnabled = _configuration.AutomaticRecoveryEnabled,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(_configuration.NetworkRecoveryIntervalSeconds)
            };


        if (_configuration.IsSSL)
        {
            _factory.Port = _configuration.TLSPort;

            _factory.Ssl = new SslOption
            {
                Enabled = _configuration.IsSSL,
                ServerName = _configuration.Hostname,
                Version = System.Security.Authentication.SslProtocols.Tls12,
            };
        }
    }

    public async Task<IConnection> OpenConnection<TEvent>(string name, string topic = null, CancellationToken cancellationToken = default) where TEvent : Event
    {
        var eventName = topic ?? typeof(TEvent).Name;
        var connectionName = name ?? eventName;

        if (!_connections.ContainsKey(connectionName) || (_connections.ContainsKey(connectionName) && !_connections[connectionName].IsOpen))
            _connections[connectionName] = await _factory.CreateConnectionAsync(cancellationToken: cancellationToken);

        return _connections[connectionName];
    }

    public async Task CloseConnection<TEvent>(string name, string topic = null, CancellationToken cancellationToken = default) where TEvent : Event
    {
        var eventName = topic ?? typeof(TEvent).Name;
        var connectionName = name ?? eventName;

        if (_connections.TryGetValue(connectionName, out var connection))
        {
            if (connection.IsOpen)
            {
                await connection.CloseAsync(cancellationToken: cancellationToken);
                connection.Dispose();
            }

        }
    }

    public void Dispose()
    {
        foreach (var conn in _connections.Values.ToList())
        {
            try
            {
                if (conn.IsOpen)
                    conn.CloseAsync().GetAwaiter().GetResult();
            }
            catch { /* Ignorar erros ao fechar */ }
            finally
            {
                conn.Dispose();
            }
        }

        _connections.Clear();
    }

    ~RabbitMQConnection() => Dispose();
}