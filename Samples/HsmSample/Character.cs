namespace HsmSample;

internal abstract class Character : Node
{
  /// <summary>The token that is rendered on the screen.</summary>
  public char Token { get; set; }

  /// <summary>The character's directional unit vector.</summary>
  /// <remarks>This value is negative if facing left, and positive when facing right.</remarks>
  public float Facing
  {
    get;
    set => field = MathF.CopySign(1, value);
  }

  /// <summary>The character's typical velocity when moving.</summary>
  public float MovementSpeed { get; set; }

  /// <summary>The character's velocity.</summary>
  /// <remarks>This value is negative if moving left, and positive when moving right.</remarks>
  public float Velocity
  {
    get;
    set
    {
      field = value;
      Facing = value;
    }
  }

  /// <summary>The distance a character can see others.</summary>
  public float SightDistance { get; set; }

  /// <summary>The number of hit points remaining.</summary>
  /// <remarks>When health reaches 0 then a character dies.</remarks>
  public int Health { get; set; }

  /// <summary>The number of hit points of damage caused by attacks.</summary>
  public int Damage { get; set; }

  /// <summary>Do damage to health.</summary>
  /// <param name="damage">The amount of damage to inflict.</param>
  public abstract void TakeDamage(int damage);

  /// <summary>Called on each frame to draw.</summary>
  public override void Draw(Renderer r)
  {
    if (!(char.IsControl(Token) || char.IsWhiteSpace(Token)))
    {
      r.DrawChar(Token, Position);
    }
  }
}
