using MarymoorStudios.Core;
using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Promises.CommandLine;
using MarymoorStudios.Core.Steamworks;
using MarymoorStudios.Core.Steamworks.Rpc;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace SteamRpcNetworkSample;

[CommandGroup("Commands for running demo host.", "host")]
internal sealed class HostDemo
{
  [Command("Runs a demo host", "run")]
  public static async Promise Run(ILoggerFactory loggerFactory, CancellationToken cancel)
  {
    try
    {
      string version = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

      // DEVELOPERS NOTE:
      //
      //   See the csproj file: You MUST provide your own steam_app.txt file for testing!
      //
      using SteamApi api =
        SteamApi.CreateGameServer(0x7f000001, 2590, 8889, EServerMode.Authentication, version);

      // Log on to an anonymous game server account.
      ISteamGameServer gs = api.Client.GetISteamGameServer();
      Console.WriteLine("Logging on...");
      await gs.UntilLogOn(default, cancel);
      Console.WriteLine($"Logged on: {gs.BLoggedOn()} ServerId: {gs.GetSteamID()} PublicIP: {gs.GetPublicIP()}");

      // Create Steam Networking Scope.
      const int maxMessageSize = 100 * 1024;
      const int chunksPerSlab = 1000;
      SteamFactoryConfig config = new();
      using MemoryPool<byte> pool = new SlabMemoryPool<byte>(maxMessageSize, chunksPerSlab);
      DemoProxy root = new(new HostDemoServer());
      await using SteamFactory<DemoServer> factory = 
        await SteamFactory.Create(config, pool, api, loggerFactory, root, cancel);

      Console.WriteLine("Initializing Steam Relay Network...");
      await api.Client.GetISteamNetworkingUtils().UntilRelayNetworkAccess(cancel);

      // Create Listener
      await using SteamListener s = await factory.ListenP2P();
      if (!api.Client.GetISteamNetworkingSockets().GetIdentity(out SteamNetworkingIdentity identity))
      {
        await Console.Error.WriteLineAsync("Failed to get listener identity");
        return;
      }
      Console.WriteLine($"Created listener: SteamID: {identity.SteamID} InstanceId: {await root.GetId()}");

      // Run until cancelled.
      await Scheduler.Run(cancel);
      Console.WriteLine("Shutting down.");
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
}
