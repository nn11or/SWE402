using System;

public class TurnManager
{
    public event Action OnTick;

    private int turnCount;

    public int TurnCount => turnCount;

    public TurnManager()
    {
        turnCount = 0;
    }

    public void Tick()
    {
        turnCount++;
        OnTick?.Invoke();
    }
}
