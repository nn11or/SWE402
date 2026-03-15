using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Background Music")]
    [SerializeField, Tooltip("Background music clip")]
    private AudioClip backgroundMusic;
    [SerializeField, Range(0f, 1f), Tooltip("Background music volume")]
    private float musicVolume = 0.3f;

    [Header("Sound Effects")]
    [SerializeField, Tooltip("Player movement sound")]
    private AudioClip playerMoveClip;
    [SerializeField, Tooltip("Wall attack/hit sound")]
    private AudioClip wallAttackClip;
    [SerializeField, Tooltip("Food pickup sound")]
    private AudioClip foodPickupClip;
    [SerializeField, Tooltip("Enemy attack sound")]
    private AudioClip enemyAttackClip;
    [SerializeField, Tooltip("Enemy death sound")]
    private AudioClip enemyDeathClip;
    [SerializeField, Tooltip("Game over sound")]
    private AudioClip gameOverClip;

    [SerializeField, Range(0f, 1f), Tooltip("SFX volume")]
    private float sfxVolume = 0.7f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Background music AudioSource on Main Camera (per assignment requirement)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            musicSource = mainCam.GetComponent<AudioSource>();
            if (musicSource == null)
                musicSource = mainCam.gameObject.AddComponent<AudioSource>();
        }
        else
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    private void Start()
    {
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayPlayerMove() => PlayClip(playerMoveClip);
    public void PlayWallAttack() => PlayClip(wallAttackClip);
    public void PlayFoodPickup() => PlayClip(foodPickupClip);
    public void PlayEnemyAttack() => PlayClip(enemyAttackClip);
    public void PlayEnemyDeath() => PlayClip(enemyDeathClip);
    public void PlayGameOver() => PlayClip(gameOverClip);
}
