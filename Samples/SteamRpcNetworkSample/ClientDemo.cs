using MarymoorStudios.Core;
using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Promises.CommandLine;
using MarymoorStudios.Core.Rpc.Exceptions;
using MarymoorStudios.Core.Steamworks;
using MarymoorStudios.Core.Steamworks.Rpc;
using Microsoft.Extensions.Logging;

namespace SteamRpcNetworkSample;

using Void = MarymoorStudios.Core.Promises.Void;

[CommandGroup("Commands for running demo client.", "client")]
internal sealed class ClientDemo
{
  [Command("Runs a demo client", "run")]
  public static async Promise Run(
    [Argument("SteamId of user to connect to")]
    ulong peer,
    [Option("Random seed", "seed")] int seed,
    ILoggerFactory loggerFactory,
    CancellationToken cancel
  )
  {
    try
    {
      Promise pendingResult;
      using (Joiner pending = Joiner.Create(out pendingResult))
      {
        // Create Steam Networking Scope.
        const int maxMessageSize = 100 * 1024;
        const int chunksPerSlab = 1000;

        // DEVELOPERS NOTE:
        //
        //   See the csproj file: You MUST provide your own steam_app.txt file for testing!
        //
        using SteamApi api = SteamApi.Create(loggerFactory);
        SteamFactoryConfig config = new();
        using MemoryPool<byte> pool = new SlabMemoryPool<byte>(maxMessageSize, chunksPerSlab);
        DemoProxy root = new(new HostDemoServer());
        await using SteamFactory<DemoServer> factory =
          await SteamFactory.Create(config, pool, api, loggerFactory, root, cancel);

        Console.WriteLine("Initializing Steam Relay Network...");
        await api.Client.GetISteamNetworkingUtils().UntilRelayNetworkAccess(cancel);

        // Connect to the host.
        Console.WriteLine($"Connecting: {peer}");
        DemoProxy remote = factory.Connect<DemoProxy, DemoServer>(new CSteamID(peer),
          localVirtualPort: 0,
          cancel);
        Guid remoteId = await remote.GetId();
        Console.WriteLine($"Connected to InstanceId: {remoteId}");

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
                string message = "Hello Steam RPC!";
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
    catch (SteamInitException ex)
    {
      Console.WriteLine($"Steam Failure: {ex.Result}: {ex.Message}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Failure: {ex.Message}");
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
