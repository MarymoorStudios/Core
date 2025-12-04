using MarymoorStudios.Core.Promises.CommandLine;

namespace HsmSample;

[CommandGroup(Name = nameof(Blinker2))]
internal sealed class Blinker2
{
  [Command]
  public static void Run(CancellationToken cancel)
  {
    TrafficLight light = new();

    DateTime last = DateTime.Now;
    while (!cancel.IsCancellationRequested)
    {
      // Send a tick event.
      DateTime next = DateTime.Now;
      light.Process(next - last);
      last = next;
    }
  }

  private sealed class TrafficLight
  {
    /// <summary>The amount of time to delay between blinks.</summary>
    private static readonly TimeSpan s_threshold = TimeSpan.FromSeconds(1);

    ////////////////////////////////////////////////////////////////////////
    // Define States
    ////////////////////////////////////////////////////////////////////////

    /// <summary>Possible behavior modes of the machine.</summary>
    private enum States
    {
      /// <summary>The light is off.</summary>
      Off,

      /// <summary>The light is on.</summary>
      On,
    }

    ////////////////////////////////////////////////////////////////////////
    // Define State Variables
    ////////////////////////////////////////////////////////////////////////

    /// <summary>The current state of the machine.</summary>
    private States m_current = States.Off;

    /// <summary>The time spent in the current state.</summary>
    private TimeSpan m_time = TimeSpan.Zero;

    ////////////////////////////////////////////////////////////////////////
    // Define Inputs
    ////////////////////////////////////////////////////////////////////////

    /// <summary>A tick event.</summary>
    public void Process(TimeSpan delta)
    {
      switch (m_current)
      {
        case States.Off:
          m_time += delta;
          if (m_time > s_threshold)
          {
            m_time = TimeSpan.Zero;
            m_current = States.On; // Transition to a new state.
            Console.WriteLine(m_current);
          }
          break;
        case States.On:
          m_time += delta;
          if (m_time > s_threshold)
          {
            m_time = TimeSpan.Zero;
            m_current = States.Off; // Transition to a new state.
            Console.WriteLine(m_current);
          }
          break;
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return m_current.ToString();
    }
  }
}
