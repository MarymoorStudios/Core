using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Rpc;
using MarymoorStudios.Core.Rpc.Identity;

namespace PromiseRpcIdentitySample;

/// <summary>Interface used to demo the Comm System.</summary>
[Eventual]
internal interface IDemoIdentity
{
  /// <summary>
  /// If authenticated, returns the user identities of the caller and the host, or <see lang="default"/>
  /// otherwise.
  /// </summary>
  /// <param name="caller">A capability to access the caller's identity.</param>
  public Promise<(UserIdentity? Caller, UserIdentity? Host)> GetIdentities(DemoIdentityProxy caller);
}

/// <summary>An implementation of <see cref="DemoIdentityServer"/> for the Host sip.</summary>
internal sealed class DemoIdentity : DemoIdentityServer
{
  private readonly UserIdentity? m_identity;

  public DemoIdentity(UserIdentity? identity)
  {
    m_identity = identity;
  }

  /// <inheritdoc/>
  public override async Promise<(UserIdentity? Caller, UserIdentity? Host)> GetIdentities(DemoIdentityProxy caller)
  {
    return (await caller.GetRemoteIdentity(), m_identity);
  }
}
