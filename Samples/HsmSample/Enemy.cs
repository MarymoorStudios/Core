using MarymoorStudios.Core;
using MarymoorStudios.Core.Fsm;

namespace HsmSample;

[Hsm]
internal sealed partial class Enemy : Character
{
  /// <summary>The amount of time the character will stand around idling.</summary>
  public BucketRateLimiter IdleTime { get; set; }

  /// <summary>The amount of time the character will remain stunned.</summary>
  public BucketRateLimiter StunTime { get; set; }

  /// <summary>The amount of time the character will remain dead (before disappearing).</summary>
  public BucketRateLimiter DeadTime { get; set; }

  /// <summary>The lower and upper bounds of the patrol region.</summary>
  public (float Lower, float Upper) PatrolLimits { get; set; }

  /// <summary>Non-null if we picked up a weapon.</summary>
  private Knife? m_weapon;

  /// <summary>The state machine.</summary>
  private readonly Hsm m_state;

  public Enemy()
  {
    Token = '#';
    Position = 50;
    Velocity = 0;
    SightDistance = 15;
    Health = 15;
    Damage = 5;

    IdleTime = new BucketRateLimiter(1, TimeSpan.FromSeconds(1.5));
    StunTime = new BucketRateLimiter(1, TimeSpan.FromSeconds(0.5));
    MovementSpeed = 5;
    PatrolLimits = (Position - 10, Position + 10);
    DeadTime = new BucketRateLimiter(1, TimeSpan.FromSeconds(1));

    m_state = Hsm.Create(this, StateMachine);
    m_weapon = null;
  }

  [HsmInput]
  public override void Process(double delta)
  {
    m_state.Process(this, delta);
  }

  [HsmInput]
  public override void TakeDamage(int damage)
  {
    m_state.TakeDamage(this, damage);
  }

  [HsmStates]
  private static void StateMachine(HsmMeta<Enemy, Hsm, State, Inputs> m, Inputs i, Enemy x)
  {
    m.Composite(State.Init,
      [
        m.Composite(State.Alive,
          [
            m.State(State.Stunned)
             .OnEnter(x.StunnedOnEnter)
             .OnExit(x.StunnedOnExit)
             .Edge(i.Process, x.StunnedProcess, State.Self, State.History),
            m.State(State.Idle)
             .OnEnter(x.IdleOnEnter)
             .Edge(i.Process, x.IdleProcess, State.Self, State.Patrol),
            m.State(State.Patrol)
             .OnEnter(x.PatrolOnEnter)
             .Edge(i.Process, x.PatrolProcess, State.Self, State.Idle),
          ])
         .OnStart(State.Idle)
         .Edge(i.TakeDamage, x.AliveTakeDamage, State.Dead, State.Stunned),
        m.State(State.Dead)
         .OnEnter(x.DeadOnEnter)
         .Edge(i.Process, x.DeadProcess),
      ])
     .OnStart(State.Alive);
  }

  private void AliveTakeDamage(Hsm m, int damage)
  {
    Health = Math.Max(0, Health - damage);
    if (Health <= 0)
    {
      Token = '&';
      m.SetNext(State.Dead);
      return;
    }
    m.SetNext(State.Stunned);
  }

  private void StunnedOnEnter(Hsm m)
  {
    StunTime.Reset();
    Token = '%';
  }

  private void StunnedOnExit(Hsm m)
  {
    Token = '#';
  }

  private void StunnedProcess(Hsm m, double delta)
  {
    if (StunTime.TryTake(delta))
    {
      m.SetNext(State.History);
      return;
    }

    m.SetNext(State.Stunned);
  }

  private void IdleOnEnter(Hsm m)
  {
    IdleTime.Reset();
    m.SetNext(State.Idle);
  }

  private void IdleProcess(Hsm m, double delta)
  {
    if (IdleTime.TryTake(delta))
    {
      m.SetNext(State.Patrol);
      return;
    }

    m.SetNext(State.Idle);
  }

  private void PatrolOnEnter(Hsm m)
  {
    if (Velocity == 0)
    {
      Velocity = Position < PatrolLimits.Upper ? MovementSpeed : -MovementSpeed;
    }
    m.SetNext(State.Patrol);
  }

  private void PatrolProcess(Hsm m, double delta)
  {
    // Move.
    Position += (float)(delta * Velocity);
    // Don't move out of patrol region.
    Position = MathF.Min(MathF.Max(Position, PatrolLimits.Lower), PatrolLimits.Upper);

    // Move weapon if held.
    if (m_weapon is not null)
    {
      m_weapon.Position = Position + Facing;
      m_weapon.Facing = Facing;

      // If the target is out of patrol range, but within sight, then throw the knife at them.
      if (SceneTree?.RayTrace<Player>(Position, Facing, SightDistance, out Player? p) ?? false)
      {
        if ((p.Position <= PatrolLimits.Lower) || (p.Position >= PatrolLimits.Upper))
        {
          m_weapon.Throw(SightDistance + 5);
          m_weapon = null;
        }
      }
    }
    else
    {
      // If next to a weapon then pick it up.
      if (SceneTree?.RayTrace<Knife>(Position, Facing, 1, out Knife? w) ?? false)
      {
        m_weapon = w;
        w.Pickup();
      }
    }

    // If outside patrol range, then stop (and later turn around).
    if ((Position <= PatrolLimits.Lower) || (Position >= PatrolLimits.Upper))
    {
      Velocity = 0;
      m.SetNext(State.Idle);
      return;
    }

    m.SetNext(State.Patrol);
  }

  private void DeadOnEnter(Hsm m)
  {
    DeadTime.Reset();
  }

  private void DeadProcess(Hsm m, double delta)
  {
    if (DeadTime.TryTake(delta))
    {
      SceneTree?.QueueFree(this);
    }
  }
}
