using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [SerializeField, Tooltip("Particle effect for wall destruction")]
    private ParticleSystem wallDestructionPrefab;
    [SerializeField, Tooltip("Particle effect for food collection")]
    private ParticleSystem foodCollectPrefab;
    [SerializeField, Tooltip("Particle effect for enemy death")]
    private ParticleSystem enemyDeathPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void PlayEffect(ParticleSystem prefab, Vector3 position)
    {
        if (prefab == null) return;

        ParticleSystem effect = Instantiate(prefab, position, Quaternion.identity);
        effect.Play();

        float duration = effect.main.duration + effect.main.startLifetime.constantMax;
        Destroy(effect.gameObject, duration);
    }

    public void PlayWallDestruction(Vector3 position) => PlayEffect(wallDestructionPrefab, position);
    public void PlayFoodCollect(Vector3 position) => PlayEffect(foodCollectPrefab, position);
    public void PlayEnemyDeath(Vector3 position) => PlayEffect(enemyDeathPrefab, position);
}
