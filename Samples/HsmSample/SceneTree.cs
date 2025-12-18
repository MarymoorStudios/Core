using MarymoorStudios.Core;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace HsmSample;

internal sealed class SceneTree : IEnumerable<Node>
{
  private readonly Queue<Node> m_toFree;
  private readonly HashSet<Node> m_free;
  private readonly List<Node> m_tree;

  private DateTime m_lastFrame;

  public SceneTree()
  {
    m_toFree = [];
    m_free = [];
    m_tree = [];
    Terminate = false;

    m_lastFrame = DateTime.Now;
  }

  /// <summary>True when the scene should terminate at the end of the current event loop.</summary>
  public bool Terminate { get; set; }

  /// <summary>Adds a node to the tree.</summary>
  /// <param name="node">The node to add.</param>
  /// <returns>True if the node was added, false otherwise.</returns>
  public bool Add(Node node)
  {
    // If it is already a member of this tree.
    if (node.SceneTree == this)
    {
      return false;
    }

    node.SceneTree?.Remove(node);
    node.SceneTree = this;
    m_tree.Add(node);
    node.Ready();
    return true;
  }

  /// <summary>Remove a node from the tree.</summary>
  /// <param name="node">The node to remove.</param>
  /// <returns>True if removed, false otherwise.</returns>
  public bool Remove(Node node)
  {
    Contract.Requires(node.SceneTree == this);
    bool removed = m_tree.Remove(node);
    Contract.Assert(removed);
    return removed;
  }

  public void QueueFree(Node node)
  {
    m_free.Add(node);
  }

  /// <summary>Runs the scene.</summary>
  /// <param name="renderer">The renderer to use.</param>
  /// <param name="cancel"></param>
  public void Run(Renderer renderer, CancellationToken cancel)
  {
    // Loop around and draw frames.
    while (!Terminate && !cancel.IsCancellationRequested)
    {
      // Handle input.
      HandleWorldInput();

      // Draw frame.
      if (!Terminate && !cancel.IsCancellationRequested)
      {
        DrawWorldFrame(renderer);
      }

      // Free resources.
      FreeResources();
    }
  }

  private void DrawWorldFrame(Renderer renderer)
  {
    DateTime now = DateTime.Now;
    TimeSpan delta = now - m_lastFrame;

    // Render the world.
    renderer.Begin();
    foreach (Node n in m_tree)
    {
      n.Draw(renderer);
    }
    renderer.End();

    // Process nodes.
    foreach (Node n in m_tree)
    {
      n.Process(delta.TotalSeconds);
    }

    // Save the time for the next frame.
    m_lastFrame = now;
  }

  private void FreeResources()
  {
    // Free resources.
    foreach (Node n in m_free)
    {
      m_toFree.Enqueue(n);
    }
    while (m_toFree.TryDequeue(out Node? n))
    {
      m_tree.Remove(n);
    }
  }

  private void HandleWorldInput()
  {
    if (Console.KeyAvailable)
    {
      ConsoleKeyInfo key = Console.ReadKey();
      foreach (Node n in m_tree)
      {
        switch (n.HandleInput(key))
        {
          case Node.InputOutcome.Handled:
            break;
          case Node.InputOutcome.Terminate:
            Terminate = true;
            return;
        }
      }
    }
  }

  /// <summary>Perform a ray trace from a position.</summary>
  /// <typeparam name="T">An (optional) filter on the type of nodes to collide with.</typeparam>
  /// <param name="position">The starting position.</param>
  /// <param name="direction">The direction to trace in.</param>
  /// <param name="maxDistance">The maximum distance to travel.</param>
  /// <param name="collision">If one found then the first node to collide with.</param>
  /// <returns>True if a node was detected by the trace, false otherwise.</returns>
  public bool RayTrace<T>(float position, float direction, float maxDistance, [MaybeNullWhen(false)] out T collision)
    where T : Node
  {
    T? closest = null;

    // Attack the closest Enemy.
    foreach (T e in m_tree.OfType<T>())
    {
      // Compute the distance from p to e.
      float distance = e.Position - position;

      // Skip those in the wrong direction.
      if ((distance * direction) < 0)
      {
        continue;
      }
      // Skip those out of range.
      if (Math.Abs(distance) > maxDistance)
      {
        continue;
      }

      if (closest is null)
      {
        closest = e;
      }
      else
      {
        // If it is closer, then make it the closest.
        if (Math.Abs(distance) < Math.Abs(closest.Position - position))
        {
          closest = e;
        }
      }
    }

    collision = closest;
    return (closest is not null);
  }

  /// <inheritdoc/>
  public IEnumerator<Node> GetEnumerator()
  {
    return m_tree.GetEnumerator();
  }

  /// <inheritdoc/>
  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}
