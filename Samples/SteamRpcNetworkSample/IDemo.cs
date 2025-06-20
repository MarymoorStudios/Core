using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Rpc;

namespace SteamRpcNetworkSample;

/// <summary>Interface used to demo the Comm System.</summary>
[Eventual]
internal interface IDemo
{
  /// <summary>Returns the server's unique diagnostic id.</summary>
  public Promise<Guid> GetId();

  /// <summary>Echos the message to the result.</summary>
  /// <param name="message">The message to echo.</param>
  /// <param name="tag">Per-call uniquifier.</param>
  /// <param name="delay">Simulated computation time.</param>
  /// <param name="shouldThrow">
  /// If true, then return a broken promise with <paramref name="message"/> as the error
  /// message, otherwise ignored.
  /// </param>
  /// <returns>Resolves to <paramref name="message"/>.</returns>
  public Promise<string> Echo(string message, Guid tag = default, TimeSpan delay = default, bool shouldThrow = false);

  /// <summary>Returns another proxy reference to the same server (self).</summary>
  /// <returns>Resolves to self.</returns>
  public DemoProxy GetSelf();

  /// <summary>Sends a promise argument.</summary>
  /// <param name="p">The promise to send.</param>
  /// <param name="tag">Per-call uniquifier.</param>
  /// <param name="delay">Simulated computation time.</param>
  /// <param name="shouldWait">
  /// If true, then the argument is awaited at the host-side before being returned,
  /// otherwise it is returned immediately without waiting locally.
  /// </param>
  /// <returns>A new promise that resolves to the same value as <paramref name="p"/>.</returns>
  public Promise SendPromise(Promise p, Guid tag = default, TimeSpan delay = default, bool shouldWait = true);

  /// <summary>Sends a promise argument.</summary>
  /// <param name="p">The promise to send.</param>
  /// <param name="tag">Per-call uniquifier.</param>
  /// <param name="delay">Simulated computation time.</param>
  /// <param name="shouldWait">
  /// If true, then the argument is awaited at the host-side before being returned,
  /// otherwise it is returned immediately without waiting locally.
  /// </param>
  /// <returns>A new promise that resolves to the same value as <paramref name="p"/>.</returns>
  public Promise<int> SendPromise(Promise<int> p, Guid tag = default, TimeSpan delay = default, bool shouldWait = true);

  /// <summary>Sends a proxy argument.</summary>
  /// <param name="p">The proxy to send.</param>
  /// <param name="tag">Per-call uniquifier.</param>
  /// <param name="delay">Simulated computation time.</param>
  /// <returns>A new proxy that resolves to the same server as <paramref name="p"/>.</returns>
  public DemoProxy SendProxy(DemoProxy p, Guid tag = default, TimeSpan delay = default);
}
