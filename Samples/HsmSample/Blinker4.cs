using MarymoorStudios.Core.Promises.CommandLine;

namespace HsmSample;

[CommandGroup(Name = nameof(Blinker4))]
internal sealed class Blinker4
{
  [Command]
  public static void Run(CancellationToken cancel)
  {
    TrafficLight light = new();

    DateTime last = DateTime.Now;
    while (!cancel.IsCancellationRequested)
    {
      // Send keyboard events (if any).
      if (Console.KeyAvailable)
      {
        ConsoleKeyInfo key = Console.ReadKey();
        if (key.Key is ConsoleKey.UpArrow or ConsoleKey.DownArrow)
        {
          light.MaintenanceMode(key.Key == ConsoleKey.DownArrow);
        }
      }

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

    /// <summary>True if in maintenance mode.</summary>
    private bool m_maintenance;

    ////////////////////////////////////////////////////////////////////////
    // Define Inputs
    ////////////////////////////////////////////////////////////////////////

    /// <summary>A maintenance input event.</summary>
    public void MaintenanceMode(bool enable)
    {
      switch (m_current)
      {
        case States.Off:
          m_maintenance = enable;
          break;
        case States.On:
          m_maintenance = enable;
          break;
      }
    }

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
            Console.WriteLine("Off");
          }
          break;
        case States.On:
          m_time += delta;
          if (m_time > s_threshold)
          {
            m_time = TimeSpan.Zero;
            m_current = States.Off; // Transition to a new state.
            if (m_maintenance)
            {
              Console.WriteLine("Red");
            }
            else
            {
              Console.WriteLine("Yellow");
            }
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
