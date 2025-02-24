using MarymoorStudios.Core;
using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Promises.CommandLine;
using MarymoorStudios.Core.Rpc.Identity;
using MarymoorStudios.Core.Rpc.Net;
using Microsoft.Extensions.Logging;
using System.Net;

namespace PromiseRpcIdentitySample;

[CommandGroup("Commands for running demo host.", "host")]
internal sealed class HostDemo
{
  [Command("Runs a demo host", "run")]
  public static async Promise Run(
    [Option("The endpoint to listen on")] string endpoint,
    AdmissionManager? admission,
    CertificateManager? certMgr,
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
    // Set admission policy to allow anyone.  The default policy allows connections ONLY with those explicitly approved.
    if (admission is not null)
    {
      admission.Policy = AdmissionManager.AdmissionPolicy.AllowAll;
    }
    Console.WriteLine($"Endpoint: {ipe}, Identity: {certMgr?.Identity}");

    // Create TCP Scope.
    const int tcpMaxMessageSize = 100 * 1024;
    const int tcpChunksPerSlab = 1000;
    TcpFactoryConfig tcpConfig = new()
    {
      CertificateManager = certMgr,
    };
    using MemoryPool<byte> pool = new SlabMemoryPool<byte>(tcpMaxMessageSize, tcpChunksPerSlab);
    DemoIdentityProxy hostObject = new(new DemoIdentity(certMgr?.Identity));
    await using TcpFactory<DemoIdentityServer> factory = new(tcpConfig, pool, loggerFactory, hostObject);

    // Create Listener
    await using TcpListener listener = await factory.Listen(ipe);

    // Run until cancelled.
    await Scheduler.Run(cancel);
    Console.WriteLine("Shutting down.");
  }
}
