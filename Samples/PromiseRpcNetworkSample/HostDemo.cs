using MarymoorStudios.Core;
using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Promises.CommandLine;
using MarymoorStudios.Core.Rpc.Net;
using Microsoft.Extensions.Logging;
using System.Net;

namespace PromiseRpcNetworkSample;

[CommandGroup("Commands for running demo host.", "host")]
internal sealed class HostDemo
{
  [Command("Runs a demo host", "run")]
  public static async Promise Run(
    [Option("The endpoint to listen on")] string endpoint,
    ILoggerFactory loggerFactory,
    CancellationToken cancel
  )
  {
    if (string.IsNullOrWhiteSpace(endpoint))
    {
      endpoint = "127.0.0.1:0";
    }
    Console.WriteLine($"Endpoint: {endpoint}");

    // Create TCP Scope.
    const int tcpMaxMessageSize = 100 * 1024;
    const int tcpChunksPerSlab = 1000;
    TcpFactoryConfig tcpConfig = new();
    using MemoryPool<byte> pool = new SlabMemoryPool<byte>(tcpMaxMessageSize, tcpChunksPerSlab);
    DemoProxy root = new(new HostDemoServer());
    await using TcpFactory<DemoServer> factory = new(tcpConfig, pool, loggerFactory, root);

    // Create Listener
    IPEndPoint ipe = IPEndPoint.Parse(endpoint);
    await using TcpListener listener = await factory.Listen(ipe);

    // Run until cancelled.
    await Scheduler.Run(cancel);
    Console.WriteLine("Shutting down.");
  }
}
