namespace HsmSample;

internal sealed class Renderer : IDisposable
{
  private readonly int m_xOrigin;
  private readonly int m_yOrigin;
  private readonly int m_width;
  private readonly char[] m_buffer;

  public Renderer()
  {
    (int _, int top) = Console.GetCursorPosition();
    m_xOrigin = 0;
    m_yOrigin = top;
    m_width = Console.WindowWidth;
    m_buffer = new char[m_width];

    // Initialize console for rendering.
    Console.CursorVisible = false;
  }

  public void Begin()
  {
    for (int i = 0; i < m_width; i++)
    {
      m_buffer[i] = ' ';
    }
  }

  public void End()
  {
    Console.SetCursorPosition(m_xOrigin, m_yOrigin);
    Console.Write(m_buffer);
  }

  public void DrawChar(char c, float position)
  {
    int x = (int)Math.Round(position, MidpointRounding.AwayFromZero);
    if ((x >= 0) && (x < m_width))
    {
      m_buffer[x] = c;
    }
  }

  public void Dispose()
  {
    Console.SetCursorPosition(m_width - 1, m_yOrigin);
    Console.WriteLine();
  }
}
