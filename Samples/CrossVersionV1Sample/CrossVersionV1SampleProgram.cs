using MarymoorStudios.Core;
using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Promises.CommandLine;
using MarymoorStudios.Core.Rpc;
using MarymoorStudios.Core.Rpc.Exceptions;
using MarymoorStudios.Core.Rpc.Net;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Net;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130
namespace CrossVersionSample;

internal static class CrossVersionV1SampleProgram
{
  private static async Task<int> Main(string[] args)
  {
    using ILoggerFactory loggingFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
       .SetMinimumLevel(LogLevel.Debug)
        // DEVELOPER NOTE: Try out https://www.nuget.org/packages/MarymoorStudios.Core.Rpc.TraceCli
       .AddEventSourceLogger()
      // DEVELOPER NOTE: Uncomment the following lines to see logging in the console window.
      //.AddSimpleConsole(options =>
      // {
      //   options.ColorBehavior = LoggerColorBehavior.Enabled;
      //   options.SingleLine = true;
      //   // ReSharper disable once StringLiteralTypo
      //   options.TimestampFormat = "[yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff] ";
      // })
    );

    RootCommand rootCommand = new("Cross-Version Demo")
    {
      new HostCmd().CreateCommandGroup(),
      new ClientCmd().CreateCommandGroup(),
    };

    CommandLineBuilder builder = new(rootCommand);
    builder.UseDefaults();
    builder.UseLogging(loggingFactory);
    Parser parser = builder.Build();
    return await parser.InvokeAsync(args);
  }
}

[CommandGroup("Commands for running cross-version client.", "client")]
internal sealed class ClientCmd
{
  [Command("Runs a cross-version client", "run")]
  public static async Promise Run(
    [Option("The endpoint to connect to", "endpoint")]
    string endpoint,
    ILoggerFactory loggerFactory,
    CancellationToken cancel
  )
  {
    if (string.IsNullOrWhiteSpace(endpoint))
    {
      endpoint = "127.0.0.1";
    }
    if (!TcpFactoryConfig.TryParseEndpoint(endpoint, out IPEndPoint? ipe))
    {
      await Console.Error.WriteLineAsync($"Invalid endpoint: {endpoint}");
      return;
    }
    Console.WriteLine($"Endpoint: {ipe}");

    // Create TCP Scope.
    const int tcpMaxMessageSize = 100 * 1024;
    const int tcpChunksPerSlab = 1000;
    TcpFactoryConfig tcpConfig = new();
    using MemoryPool<byte> pool = new SlabMemoryPool<byte>(tcpMaxMessageSize, tcpChunksPerSlab);
    NothingProxy clientRoot = new(new RemotedException("Client doesn't export anything interface"));
    await using TcpFactory<NothingServer> factory = new(tcpConfig, pool, loggerFactory, clientRoot);

    // Connect to the host.
    MetadataProxy remote = factory.Connect<MetadataProxy, MetadataServer>(ipe, cancel);
    Console.WriteLine($"Connecting: {ipe}");

    try
    {
      foreach (IMetadata.Descriptor cap in await remote.GetCapabilities())
      {
        switch (cap)
        {
          case CrossDescriptor c:
          {
            Guid tag = Guid.NewGuid();
            CrossValue input = new("Hello from CrossVersion Client V1!");
            Console.WriteLine($"[{tag}] Input: {input}");
            CrossValue retval = await c.Capability.Call(input, tag);
            Console.WriteLine($"[{tag}] Retval: {retval}");
            break;
          }
        }
      }
    }
    catch (AbortedException)
    {
      Console.WriteLine("Disconnected");
    }

    Console.WriteLine("Shutting down.");
  }
}

[CommandGroup("Commands for running cross-version host.", "host")]
internal sealed class HostCmd
{
  [Command("Runs a cross-version host", "run")]
  public static async Promise Run(
    [Option("The endpoint to listen on")] string endpoint,
    ILoggerFactory loggerFactory,
    CancellationToken cancel
  )
  {
    if (string.IsNullOrWhiteSpace(endpoint))
    {
      endpoint = "127.0.0.1";
    }
    if (!TcpFactoryConfig.TryParseEndpoint(endpoint, out IPEndPoint? ipe))
    {
      await Console.Error.WriteLineAsync($"Invalid endpoint: {endpoint}");
      return;
    }
    Console.WriteLine($"Endpoint: {ipe}");

    // Create TCP Scope.
    const int tcpMaxMessageSize = 100 * 1024;
    const int tcpChunksPerSlab = 1000;
    TcpFactoryConfig tcpConfig = new();
    using MemoryPool<byte> pool = new SlabMemoryPool<byte>(tcpMaxMessageSize, tcpChunksPerSlab);
    await using TcpFactory<MetadataServer> factory = new(tcpConfig,
      pool,
      loggerFactory,
      new MetadataPublisher([new CrossDescriptor("ICross", "V1", new CrossProxy(new CrossObject()))]).Self);

    // Create Listener
    await using TcpListener listener = await factory.Listen(ipe);

    // Run until cancelled.
    await Scheduler.Run(cancel);
    Console.WriteLine("Shutting down.");
  }
}
