# Add Connection Lifecycle Management to MCP Client

## Summary
This PR adds connection lifecycle event handling to the MCP C# SDK client, enabling applications to properly detect and respond to connection state changes. This is essential for building robust production applications that need to handle network interruptions, server restarts, and graceful disconnections.

## Motivation
Currently, MCP client applications have no standardized way to:
- Detect when a connection is established or lost
- Implement automatic reconnection logic
- Distinguish between graceful and unexpected disconnections
- Monitor connection health across different transport types

This limitation makes it difficult to build production-ready applications that can handle real-world network conditions.

## Changes

### Core API Additions
- Added `IsConnected` property to `IMcpClient` for synchronous connection status checks
- Added three lifecycle events to `IMcpClient`:
  - `Connected` - Fired when connection is successfully established
  - `Disconnected` - Fired when connection is terminated (graceful or unexpected)
  - `ConnectionError` - Fired when connection errors occur

### Implementation Details
- Leverages existing `MessageProcessingTask` for disconnection detection (no additional threads)
- Properly detects process termination for STDIO transports via `Process.HasExited`
- Thread-safe event handling using `Interlocked` operations
- Consistent logging using `LoggerMessage` attributes matching upstream patterns
- Zero performance overhead when events are not subscribed

### Transport Support
- ✅ STDIO transport - Full support with process monitoring
- ✅ HTTP/SSE transport - Full support  
- ✅ StreamableHttp transport - Full support
- All transports tested and verified

## Usage Example

```csharp
var mcpClient = await McpClientFactory.CreateAsync(transport);

// Subscribe to lifecycle events
mcpClient.Connected += (sender, e) => 
    Console.WriteLine($"Connected to {e.ServerInfo.Name} at {e.ConnectedAt}");

mcpClient.Disconnected += async (sender, e) =>
{
    if (!e.IsGraceful)
    {
        Console.WriteLine($"Unexpected disconnection: {e.Error?.Message}");
        // Implement reconnection logic
        await ReconnectWithBackoff();
    }
};

// Check connection status
if (mcpClient.IsConnected)
{
    await mcpClient.SendRequestAsync(...);
}
```

## Testing
- Added comprehensive unit tests for all event scenarios
- Added integration tests for each transport type
- Created sample application demonstrating reconnection patterns
- All existing tests continue to pass

## Documentation
- Added conceptual documentation in `docs/concepts/connection-lifecycle/`
- Updated API documentation with event descriptions
- Created sample application showing best practices
- Added migration guide for existing applications

## Breaking Changes
None. All changes are additive and backward compatible.

## Checklist
- [x] Code follows upstream coding style and conventions
- [x] All tests pass
- [x] Documentation is complete
- [x] Sample application runs correctly
- [x] No breaking changes to existing API
- [x] Logging follows upstream patterns using LoggerMessage
- [x] Thread-safe implementation
- [x] No unnecessary allocations or performance overhead
- [x] Works across all transport types

## Related Issues
This addresses common community requests for connection management capabilities.