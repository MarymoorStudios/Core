using MarymoorStudios.Core;
using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Promises.CommandLine;
using MarymoorStudios.Core.Rpc.Exceptions;
using MarymoorStudios.Core.Rpc.Net;
using Microsoft.Extensions.Logging;
using System.Net;

namespace PromiseRpcNetworkSample;

using Void = MarymoorStudios.Core.Promises.Void;

[CommandGroup("Commands for running demo client.", "client")]
internal sealed class ClientDemo
{
  [Command("Runs a demo client", "run")]
  public static async Promise Run(
    [Option("The endpoint to connect to", "endpoint")]
    string endpoint,
    [Option("Random seed", "seed")] int seed,
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

    Promise pendingResult;
    using (Joiner pending = Joiner.Create(out pendingResult))
    {
      // Create TCP Scope.
      const int tcpMaxMessageSize = 100 * 1024;
      const int tcpChunksPerSlab = 1000;
      TcpFactoryConfig tcpConfig = new();
      using MemoryPool<byte> pool = new SlabMemoryPool<byte>(tcpMaxMessageSize, tcpChunksPerSlab);
      DemoProxy root = new(new HostDemoServer());
      await using TcpFactory<DemoServer> factory = new(tcpConfig, pool, loggerFactory, root);

      // Connect to the host.
      DemoProxy remote = factory.Connect<DemoProxy, DemoServer>(ipe, cancel);
      Console.WriteLine($"Connecting: {ipe}");
      Guid remoteId = await remote.GetId();
      Console.WriteLine($"Connected: {remoteId}");

      // Create a pseudo-random number generator.
      Random rand = new(seed);

      // Run until cancelled or disconnected.
      try
      {
        while (!cancel.IsCancellationRequested)
        {
          switch (rand.NextEnum<Actions>())
          {
            case Actions.Wait:
            {
              TimeSpan delay = TimeSpan.FromMilliseconds(750);
              await Scheduler.Delay(delay);
              break;
            }
            case Actions.Echo:
            {
              Guid tag = Guid.NewGuid();
              string message = "Hello Promise RPC!";
              TimeSpan delay = TimeSpan.FromMilliseconds(500);
              string r = await remote.Echo(message, tag, delay);
              Contract.Invariant(r == message);
              break;
            }
            case Actions.SendPromise:
            {
              Guid tag = Guid.NewGuid();
              Resolver<Void> r = new();
              Promise p = new(r);
              TimeSpan delay = TimeSpan.FromMilliseconds(500);
              bool shouldWait = rand.NextBool();
              TimeSpan delay2 = TimeSpan.FromMilliseconds(500);
              Promise q = remote.SendPromise(p, tag, delay, shouldWait);
              // Delay resolving the argument.
              pending.Link(Scheduler.Delay(delay2).When(r.Resolve));
              // Delay waiting on the return value.
              pending.Link(q);
              break;
            }
            case Actions.SendDataPromise:
            {
              Guid tag = Guid.NewGuid();
              Resolver<int> r = new();
              Promise<int> p = new(r);
              TimeSpan delay = TimeSpan.FromMilliseconds(500);
              bool shouldWait = rand.NextBool();
              TimeSpan delay2 = TimeSpan.FromMilliseconds(500);
              Promise<int> q = remote.SendPromise(p, tag, delay, shouldWait);
              // Delay resolving the argument.
              pending.Link(Scheduler.Delay(delay2).When(() => r.Resolve(42)));
              // Delay waiting on the return value (and verify its value).
              pending.Link(q.When(x => { Contract.Invariant(x == 42); }));
              break;
            }
            case Actions.SendProxy:
            {
              Guid tag = Guid.NewGuid();
              Resolver<DemoServer> r = new();
              DemoProxy p = new(r);
              TimeSpan delay = TimeSpan.FromMilliseconds(500);
              TimeSpan delay2 = TimeSpan.FromMilliseconds(500);
              DemoProxy q = remote.SendProxy(p, tag, delay);
              HostDemoServer s = new();
              // Delay resolving the argument.
              pending.Link(Scheduler.Delay(delay2).When(() => r.Resolve(s)));
              // Delay waiting on the return value (and verify its value).
              pending.Link(q.GetId().When(async id => { Contract.Invariant(id == await s.GetId()); }));
              break;
            }
            default:
              throw Contract.FailThrow("Invalid");
          }
        }
      }
      catch (AbortedException)
      {
        Console.WriteLine("Disconnected");
      }
    }

    Console.WriteLine("Shutting down.");
    try
    {
      await pendingResult;
    }
    catch (Exception)
    {
      // ignore errors on exit (because the channels are likely shutting down breaking all results).
    }
  }

  private enum Actions
  {
    Wait,
    Echo,
    SendPromise,
    SendDataPromise,
    SendProxy,
  }
}
