using UnityEngine;

public class WallObject : CellObject
{
    [SerializeField, Range(1, 10), Tooltip("Hit points for this wall")]
    private int hitPoints = 3;

    [SerializeField, Tooltip("Damaged wall sprite (shown after taking damage)")]
    private Sprite damagedSprite;

    private SpriteRenderer spriteRenderer;
    private int currentHP;

    public override void Init(int x, int y)
    {
        base.Init(x, y);
        currentHP = hitPoints;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override bool PlayerWantsToEnter()
    {
        currentHP--;
        AudioManager.Instance?.PlayWallAttack();

        if (currentHP <= 0)
        {
            VFXManager.Instance?.PlayWallDestruction(transform.position);
            GameManager.Instance.BoardManager.SetCellObject(cellX, cellY, null);

            ObjectPool pool = FindFirstObjectByType<ObjectPool>();
            if (pool != null)
                pool.ReturnToPool(this);
            else
                Destroy(gameObject);

            return true;
        }

        if (damagedSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = damagedSprite;

        return false;
    }
}
