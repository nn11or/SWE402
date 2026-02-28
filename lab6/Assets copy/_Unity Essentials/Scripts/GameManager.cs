using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static event Action OnGameOver;

    private static bool raised;

    public static void RaiseGameOver()
    {
        if (raised) return;
        raised = true;
        OnGameOver?.Invoke();
    }

    void Start()
    {
        raised = false;
    }
}