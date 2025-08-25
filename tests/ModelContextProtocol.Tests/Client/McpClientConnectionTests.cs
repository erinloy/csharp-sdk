using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Tests.Utils;

namespace ModelContextProtocol.Tests.Client;

public class McpClientConnectionTests
{
    private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;
    
    public McpClientConnectionTests()
    {
        _loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => { });
    }

    [Fact]
    public void IsConnected_WhenNotConnected_ReturnsFalse()
    {
        // Arrange
        var transport = new TestServerTransport();
        using var client = new McpClient(new StreamClientTransport(transport, transport), null, _loggerFactory);

        // Act & Assert
        Assert.False(client.IsConnected);
    }

    [Fact]
    public void Connected_Event_IsAvailable()
    {
        // Arrange
        var transport = new TestServerTransport();
        using var client = new McpClient(new StreamClientTransport(transport, transport), null, _loggerFactory);

        ConnectionEventArgs? eventArgs = null;
        
        // Act - Subscribe to event (should not throw)
        client.Connected += (sender, args) => eventArgs = args;
        
        // Assert
        Assert.Null(eventArgs); // No connection made yet
    }

    [Fact]
    public void Disconnected_Event_IsAvailable()
    {
        // Arrange
        var transport = new TestServerTransport();
        using var client = new McpClient(new StreamClientTransport(transport, transport), null, _loggerFactory);

        DisconnectionEventArgs? eventArgs = null;
        
        // Act - Subscribe to event (should not throw)
        client.Disconnected += (sender, args) => eventArgs = args;
        
        // Assert
        Assert.Null(eventArgs); // No disconnection yet
    }

    [Fact]
    public void ConnectionError_Event_IsAvailable()
    {
        // Arrange
        var transport = new TestServerTransport();
        using var client = new McpClient(new StreamClientTransport(transport, transport), null, _loggerFactory);

        ConnectionErrorEventArgs? eventArgs = null;
        
        // Act - Subscribe to event (should not throw)
        client.ConnectionError += (sender, args) => eventArgs = args;
        
        // Assert
        Assert.Null(eventArgs); // No connection error yet
    }

    [Fact]
    public async Task ConnectionError_Event_IsFiredOnConnectionFailure()
    {
        // Arrange
        var faultyTransport = new FaultyClientTransport();
        using var client = new McpClient(faultyTransport, null, _loggerFactory);

        ConnectionErrorEventArgs? errorArgs = null;
        client.ConnectionError += (sender, args) => errorArgs = args;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.ConnectAsync());
        Assert.NotNull(errorArgs);
        Assert.Equal(exception, errorArgs.Error);
    }

    /// <summary>
    /// Test transport that always fails to connect
    /// </summary>
    private class FaultyClientTransport : IClientTransport
    {
        public string Name => "faulty-test-transport";

        public Task<ITransport> ConnectAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Test connection failure");
        }
    }
}