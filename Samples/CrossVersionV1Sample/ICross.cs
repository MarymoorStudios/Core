using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Rpc;
using MarymoorStudios.Core.Serialization;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130
namespace CrossVersionSample;

/// <summary>Interface used to demo the cross-version features of Comm System.</summary>
[Eventual]
internal partial interface ICross
{
  public Promise<CrossValue> Call(CrossValue input, Guid tag);
}

/// <summary>A data contract used in <see cref="ICross"/>.</summary>
[DataContract]
internal sealed record CrossValue(string Message);

/// <summary>A capability for exporting a <see cref="ICross"/>.</summary>
[DataContract]
internal sealed record CrossCapability(string Name, string Description, CrossProxy Capability)
  : ICapability.Descriptor(Name, Description);

/// <summary>An implementation of <see cref="ICross"/>.</summary>
internal sealed class CrossObject : CrossServer
{
  /// <inheritdoc/>
  public override Promise<CrossValue> Call(CrossValue input, Guid tag)
  {
    Console.WriteLine($"[{tag}] Input: {input}");
    CrossValue retval = new("Hello back from CrossObject V1!");
    Console.WriteLine($"[{tag}] Retval: {retval}");
    return Promise.From(retval);
  }
}
