# Suggested Commit Message

```
feat: Add connection lifecycle management to MCP client

Adds comprehensive connection lifecycle event handling to enable robust
production applications that can properly detect and respond to connection
state changes.

Key features:
- Added Connected, Disconnected, and ConnectionError events to IMcpClient
- Added IsConnected property for synchronous connection status checks  
- Proper process monitoring for STDIO transport via Process.HasExited
- Thread-safe implementation using Interlocked operations
- Zero overhead when events are not subscribed

Implementation leverages existing MessageProcessingTask for disconnection
detection without additional threads. All transports (STDIO, HTTP/SSE,
StreamableHttp) are fully supported and tested.

No breaking changes - all additions are backward compatible.
```

## Alternative Conventional Commit Format

```
feat(client): add connection lifecycle management

- add Connected/Disconnected/ConnectionError events
- add IsConnected property for status checks
- implement process monitoring for STDIO transport
- ensure thread-safe event handling
- support all transport types

BREAKING CHANGE: none
```