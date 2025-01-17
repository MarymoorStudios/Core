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
  public Promise<CrossValue> CallV1(CrossValue input, Guid tag);
}

/// <summary>A future version of <see cref="ICross"/> with extra methods.</summary>
[Eventual]
internal partial interface ICross2
{
  public Promise<CrossValue> CallV1(CrossValue input, Guid tag);

  public Promise<CrossValue> CallV2(CrossValue input, Guid tag);
}

/// <summary>A future version of a data contract with extra properties.</summary>
[DataContract]
internal sealed record CrossValue(string Message, int Code);

/// <summary>A capability for exporting a <see cref="ICross"/>.</summary>
[DataContract]
internal sealed record CrossCapability(string Name, string Description, CrossProxy Capability)
  : ICapability.Descriptor(Name, Description);

/// <summary>A capability for exporting a <see cref="ICross2"/>.</summary>
[DataContract]
internal sealed record Cross2Capability(string Name, string Description, Cross2Proxy Capability)
  : ICapability.Descriptor(Name, Description);

/// <summary>An implementation of <see cref="ICross"/>.</summary>
internal sealed class CrossObject : CrossServer
{
  /// <inheritdoc/>
  public override Promise<CrossValue> CallV1(CrossValue input, Guid tag)
  {
    Console.WriteLine($"[{tag}] Input: {input}");
    CrossValue retval = new("Hello back from CrossObject V2!", 42);
    Console.WriteLine($"[{tag}] Retval: {retval}");
    return Promise.From(retval);
  }
}

/// <summary>An implementation of <see cref="ICross2"/>.</summary>
internal sealed class Cross2Object : Cross2Server
{
  /// <inheritdoc/>
  public override Promise<CrossValue> CallV1(CrossValue input, Guid tag)
  {
    Console.WriteLine($"[{tag}] Input: {input}");
    CrossValue retval = new("Hello back from Cross2Object V2!", 42);
    Console.WriteLine($"[{tag}] Retval: {retval}");
    return Promise.From(retval);
  }

  /// <inheritdoc />
  public override Promise<CrossValue> CallV2(CrossValue input, Guid tag)
  {
    Console.WriteLine($"[{tag}] Input: {input}");
    CrossValue retval = new("Goodbye from Cross2Object V2!", 42);
    Console.WriteLine($"[{tag}] Retval: {retval}");
    return Promise.From(retval);
  }
}
