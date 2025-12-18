namespace HsmSample;

internal abstract class Node
{
  /// <summary>The scene tree this node belongs to.</summary>
  public SceneTree? SceneTree { get; set; }

  /// <summary>The position in the 1-dimensional game world.</summary>
  /// <remarks>The x-axis increases to the right.</remarks>
  public float Position { get; set; }

  /// <summary>Outcomes for handling input.</summary>
  public enum InputOutcome
  {
    /// <summary>Input was not handled.</summary>
    None,

    /// <summary>Input was handled.  No further objects should see this input.</summary>
    Handled,

    /// <summary>Input was handled.  Game should exit.</summary>
    Terminate,
  }

  /// <summary>Handle keyboard input.</summary>
  public virtual InputOutcome HandleInput(ConsoleKeyInfo key)
  {
    return InputOutcome.None;
  }

  /// <summary>Called on each frame to draw.</summary>
  public virtual void Draw(Renderer r)
  {
  }

  /// <summary>Called when the node is added to the scene tree.</summary>
  public virtual void Ready()
  {
  }

  /// <summary>Called on each frame to perform per-frame processing.</summary>
  public virtual void Process(double delta)
  {
  }
}
