using MarymoorStudios.Core;
using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Promises.CommandLine;
using MarymoorStudios.Core.Rpc.Identity;
using MarymoorStudios.Core.Rpc.Net;
using Microsoft.Extensions.Logging;
using System.Net;

namespace PromiseRpcIdentitySample;

[CommandGroup("Commands for running demo client.", "client")]
internal sealed class ClientDemo
{
  [Command("Runs a demo client", "run")]
  public static async Promise Run(
    [Option("The endpoint to connect to", "endpoint")]
    string endpoint,
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
    DemoIdentityProxy clientObject = new(new DemoIdentity(certMgr?.Identity));
    await using TcpFactory<DemoIdentityServer> factory = new(tcpConfig, pool, loggerFactory, clientObject);

    // Connect to the host.
    DemoIdentityProxy remote = factory.Connect<DemoIdentityProxy, DemoIdentityServer>(ipe, cancel);
    Console.WriteLine($"Connecting: {ipe}");
    (UserIdentity? caller, UserIdentity? host) = await remote.GetIdentities(clientObject);
    Console.WriteLine($"Caller: {caller}, Host: {host}");
  }
}
