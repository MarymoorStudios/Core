using MarymoorStudios.Core;
using MarymoorStudios.Core.Fsm;
using MarymoorStudios.Core.Promises.CommandLine;
using System.Numerics;

namespace HsmSample;

[CommandGroup(Name = nameof(Hero2))]
internal sealed partial class Hero2
{
  [Command]
  public static void Run(CancellationToken cancel)
  {
    HeroHsm h = new();
    Enemy e = new();

    DateTime last = DateTime.Now;
    while (!cancel.IsCancellationRequested)
    {
      // Send keyboard events (if any).
      if (Console.KeyAvailable)
      {
        ConsoleKeyInfo key = Console.ReadKey();
        switch (key.Key)
        {
          case ConsoleKey.UpArrow:
            h.Jump();
            break;
          case ConsoleKey.DownArrow:
            h.Slide();
            break;
          case ConsoleKey.RightArrow:
            h.Attack(e);
            break;
          case ConsoleKey.LeftArrow:
            h.TakeDamage(10);
            break;
        }
      }

      // Send a tick event.
      DateTime next = DateTime.Now;
      h.Process(next - last);
      last = next;
    }
  }

  [Hsm]
  private sealed partial class HeroHsm
  {
    /// <summary>The number of hit points of damaged caused by the hero's weapon.</summary>
    private const int s_attackDamage = 10;

    /// <summary>The force of a jump.</summary>
    private static readonly Vector2 s_jumpVelocity = new(0, 30);

    /// <summary>The force of a slide.</summary>
    private static readonly Vector2 s_slideDisplacement = new(20, 0);

    /// <summary>Default acceleration of gravity.</summary>
    private static readonly Vector2 s_gravity = new(0, -10);

    /// <summary>The size of the play area.</summary>
    private static readonly Vector2 s_screenSize = new(100, 100);

    ////////////////////////////////////////////////////////////////////////
    // Define States
    ////////////////////////////////////////////////////////////////////////

    [HsmStates]
    private static void StateMachine(HsmMeta<HeroHsm, Hsm, State, Inputs> m, Inputs i, HeroHsm x)
    {
      m.Composite(State.Init,
        [
          m.Composite(State.Alive,
            [
              m.State(State.Ground)
               .Edge(i.Jump, x.GroundJump, State.Airborne)
               .Edge(i.Slide, x.GroundSlide),
              m.Composite(State.Airborne,
                [
                  m.State(State.Flying)
                   .Edge(i.Jump, x.FlyingJump, State.Airborne),
                ])
               .Edge(i.Jump, x.AirborneJump, State.Flying)
               .Edge(i.Slide, x.AirborneSlide),
            ])
           .Edge(i.Attack, x.AliveAttack)
           .Edge(i.TakeDamage, x.AliveTakeDamage, State.Self, State.Downed)
           .Edge(i.Process, x.AliveProcess, State.Self, State.Ground),
          m.State(State.Downed)
           .Edge(i.Process, x.DownedProcess),
        ])
       .OnStart(State.Ground);
    }

    private void AliveAttack(Hsm m, Enemy e)
    {
      e.TakeDamage(s_attackDamage);
      Console.WriteLine($"{this} Attack: {s_attackDamage}");

      // Apply "reflect" damage.
      TakeDamage(5);
    }

    private void AliveTakeDamage(Hsm m, int damage)
    {
      m_health -= damage;
      Console.WriteLine($"{this} Damaged: {damage}");
      if (m_health <= 0)
      {
        m.SetNext(State.Downed);
      }
    }

    private void AliveProcess(Hsm m, TimeSpan delta)
    {
      // Move the character...
      m_position += (float)delta.TotalSeconds * m_velocity;
      m_position = Vector2.Clamp(m_position, Vector2.Zero, s_screenSize); // Stay on the screen.
      if (m_position.Y == 0)
      {
        m.SetNext(State.Ground);
      }
      // Apply gravity.
      m_velocity += (float)delta.TotalSeconds * m_gravity;
      m_velocity = Vector2.Clamp(m_velocity, s_gravity, s_jumpVelocity);

      // Render the character.
      if (m_printRate.TryTake(delta))
      {
        Console.WriteLine($"{this} Position: {m_position}");
      }
    }

    private void GroundJump(Hsm m)
    {
      // Jump upwards.
      m_velocity += s_jumpVelocity;
      m_gravity = s_gravity;
      m.SetNext(State.Airborne);
    }

    private void GroundSlide(Hsm m)
    {
      // Slide sideways.
      m_position += s_slideDisplacement;
    }

    private void AirborneJump(Hsm m)
    {
      // Jump while airborne leads to flying.
      m_velocity *= Vector2.UnitX;
      m_gravity = Vector2.Zero;
      m.SetNext(State.Flying);
    }

    private void AirborneSlide(Hsm m)
    {
      // Ignore.  Can't slide while airborne.
    }

    private void FlyingJump(Hsm m)
    {
      // Jump while flying leads to falling back down.
      m_gravity = s_gravity;
      m.SetNext(State.Airborne);
    }

    private void DownedProcess(Hsm m, TimeSpan delta)
    {
      if (m_printRate.TryTake(delta))
      {
        Console.WriteLine("DOWNED");
      }
    }

    ////////////////////////////////////////////////////////////////////////
    // Define State Variables
    ////////////////////////////////////////////////////////////////////////

    /// <summary>The current state of the machine.</summary>
    private readonly Hsm m_current;

    /// <summary>The number of hit points remaining.</summary>
    private int m_health = 20;

    /// <summary>The player's current position.</summary>
    private Vector2 m_position = Vector2.Zero;

    /// <summary>The upward velocity.</summary>
    /// <remarks>This value is negative when travelling down.</remarks>
    private Vector2 m_velocity = Vector2.Zero;

    /// <summary>The velocity due gravity.</summary>
    private Vector2 m_gravity = s_gravity;

    /// <summary>A rate limiter for how often to print (during Process).</summary>
    private readonly BucketRateLimiter m_printRate = new(1, TimeSpan.FromSeconds(1), 1);

    public HeroHsm()
    {
      m_current = Hsm.Create(this, StateMachine);
    }

    ////////////////////////////////////////////////////////////////////////
    // Define Inputs
    ////////////////////////////////////////////////////////////////////////

    /// <summary>Called when the attack action is triggered.</summary>
    [HsmInput]
    public void Attack(Enemy e)
    {
      m_current.Attack(this, e);
    }

    /// <summary>Called when attacked by an enemy.</summary>
    [HsmInput]
    public void TakeDamage(int damage)
    {
      m_current.TakeDamage(this, damage);
    }

    /// <summary>Called when the jump action is triggered.</summary>
    [HsmInput]
    public void Jump()
    {
      m_current.Jump(this);
    }

    /// <summary>Called when the slide action is triggered.</summary>
    [HsmInput]
    public void Slide()
    {
      m_current.Slide(this);
    }

    /// <summary>Called on each tick.</summary>
    [HsmInput]
    public void Process(TimeSpan delta)
    {
      m_current.Process(this, delta);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return $"{m_current.CurrentState.Name}";
    }
  }
}
