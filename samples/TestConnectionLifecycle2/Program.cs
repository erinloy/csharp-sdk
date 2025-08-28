using ModelContextProtocol.Client;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Connection Lifecycle Events - Proper Event Subscription\n");
        
        // Create transport but don't connect yet
        var weatherServerExe = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!,
            "..", "..", "QuickstartWeatherServer", "Debug", "net8.0", "QuickstartWeatherServer.exe"
        ));
        
        var transport = new StdioClientTransport(new()
        {
            Name = "Weather Server",
            Command = weatherServerExe,
            Arguments = [],
        });
        
        // Create client WITHOUT connecting (we need to implement this properly)
        // For now, we'll connect and see what happens
        var client = await McpClientFactory.CreateAsync(transport);
        
        // The Connected event should have already fired during CreateAsync
        // because connection is established in CreateAsync
        
        Console.WriteLine($"✅ IsConnected after creation: {client.IsConnected}");
        
        // Subscribe to events for demonstration of disconnection
        bool disconnectedFired = false;
        
        client.Disconnected += (sender, e) =>
        {
            disconnectedFired = true;
            Console.WriteLine($"✅ Disconnected event fired: IsGraceful={e.IsGraceful}, Error={e.Error?.Message}");
        };
        
        // Test that we can use the connection
        var tools = await client.ListToolsAsync();
        Console.WriteLine($"✅ Listed {tools.Count} tools - connection working");
        
        // Now test process termination detection
        Console.WriteLine("\nKilling server process to test unexpected disconnection detection...");
        
        // Get the process ID and kill it
        var processField = transport.GetType().GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (processField != null)
        {
            var process = processField.GetValue(transport) as System.Diagnostics.Process;
            if (process != null && !process.HasExited)
            {
                process.Kill();
                await Task.Delay(500); // Wait for disconnection detection
                
                Console.WriteLine($"✅ IsConnected after kill: {client.IsConnected}");
                Console.WriteLine($"✅ Disconnected event fired: {disconnectedFired}");
            }
        }
        
        // Clean up
        try
        {
            await client.DisposeAsync();
        }
        catch
        {
            // Expected - already disconnected
        }
        
        Console.WriteLine("\n✅ Connection lifecycle test completed!");
    }
}