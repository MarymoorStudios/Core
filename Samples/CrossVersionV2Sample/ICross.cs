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

/// <summary>A future version of <see cref="ICross"/> with extra methods.</summary>
[Eventual]
internal partial interface IExtraCross
{
  public Promise<CrossValue> CallExtra(CrossValue input, Guid tag);
}

/// <summary>A future version of a data contract with extra properties.</summary>
[DataContract]
internal sealed record CrossValue(string Message, int Code);

/// <summary>A capability for exporting a <see cref="ICross"/>.</summary>
[DataContract]
internal sealed record CrossDescriptor(string Name, string Description, CrossProxy Capability)
  : IMetadata.Descriptor(Name, Description);

/// <summary>A capability for exporting a <see cref="IExtraCross"/>.</summary>
[DataContract]
internal sealed record ExtraCrossDescriptor(string Name, string Description, ExtraCrossProxy Capability)
  : IMetadata.Descriptor(Name, Description);

/// <summary>An implementation of <see cref="ICross"/>.</summary>
internal sealed class CrossObject : CrossServer
{
  /// <inheritdoc/>
  public override Promise<CrossValue> Call(CrossValue input, Guid tag)
  {
    Console.WriteLine($"[{tag}] Input: {input}");
    CrossValue retval = new("Hello back from CrossObject V2!", 42);
    Console.WriteLine($"[{tag}] Retval: {retval}");
    return Promise.From(retval);
  }
}

/// <summary>An implementation of <see cref="IExtraCross"/>.</summary>
internal sealed class ExtraCrossObject : ExtraCrossServer
{
  /// <inheritdoc />
  public override Promise<CrossValue> CallExtra(CrossValue input, Guid tag)
  {
    Console.WriteLine($"[{tag}] Input: {input}");
    CrossValue retval = new("Here something extra from ExtraCrossObject V2!", 42);
    Console.WriteLine($"[{tag}] Retval: {retval}");
    return Promise.From(retval);
  }
}
