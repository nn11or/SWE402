using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private AudioSource audioSource;

    void OnEnable()
    {
        GameManager.OnGameOver += HandleGameOver;
    }

    void OnDisable()
    {
        GameManager.OnGameOver -= HandleGameOver;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void HandleGameOver()
    {
        if (audioSource != null) audioSource.Stop();
    }
}