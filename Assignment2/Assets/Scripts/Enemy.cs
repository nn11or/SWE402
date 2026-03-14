using UnityEngine;

public class Enemy : CellObject
{
    [SerializeField, Range(1, 10), Tooltip("Hit points for this enemy")]
    private int hitPoints = 3;

    [SerializeField, Range(1, 20), Tooltip("Damage dealt to player food on attack")]
    private int attackDamage = 5;

    private int currentHP;
    private Animator animator;

    public override void Init(int x, int y)
    {
        base.Init(x, y);
        currentHP = hitPoints;
        animator = GetComponent<Animator>();

        GameManager.Instance.TurnManager.OnTick += OnTurnTick;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
            GameManager.Instance.TurnManager.OnTick -= OnTurnTick;
    }

    public override bool PlayerWantsToEnter()
    {
        currentHP--;
        AudioManager.Instance?.PlayWallAttack();

        if (animator != null)
            animator.SetTrigger("Hit");

        if (currentHP <= 0)
        {
            AudioManager.Instance?.PlayEnemyDeath();
            VFXManager.Instance?.PlayEnemyDeath(transform.position);
            GameManager.Instance.TurnManager.OnTick -= OnTurnTick;
            GameManager.Instance.BoardManager.SetCellObject(cellX, cellY, null);

            ObjectPool pool = FindFirstObjectByType<ObjectPool>();
            if (pool != null)
                pool.ReturnToPool(this);
            else
                Destroy(gameObject);

            return true;
        }

        return false;
    }

    private void OnTurnTick()
    {
        if (!gameObject.activeInHierarchy) return;

        PlayerController player = GameManager.Instance.PlayerController;
        if (player == null) return;

        int playerX = player.CellX;
        int playerY = player.CellY;

        int dx = playerX - cellX;
        int dy = playerY - cellY;

        // If adjacent to player, attack
        if (Mathf.Abs(dx) + Mathf.Abs(dy) == 1)
        {
            GameManager.Instance.ChangeFood(-attackDamage);
            AudioManager.Instance?.PlayEnemyAttack();

            if (animator != null)
                animator.SetTrigger("Attack");
            return;
        }

        // Move toward player
        int moveX = 0;
        int moveY = 0;

        if (Mathf.Abs(dx) > Mathf.Abs(dy))
            moveX = dx > 0 ? 1 : -1;
        else if (dy != 0)
            moveY = dy > 0 ? 1 : -1;
        else
            moveX = dx > 0 ? 1 : -1;

        int newX = cellX + moveX;
        int newY = cellY + moveY;

        BoardManager.CellData targetCell = GameManager.Instance.BoardManager.GetCellData(newX, newY);

        if (targetCell != null && targetCell.Passable && targetCell.ContainedObject == null)
        {
            // Check if player is there
            if (newX == playerX && newY == playerY)
            {
                GameManager.Instance.ChangeFood(-attackDamage);
                AudioManager.Instance?.PlayEnemyAttack();

                if (animator != null)
                    animator.SetTrigger("Attack");
                return;
            }

            GameManager.Instance.BoardManager.SetCellObject(cellX, cellY, null);
            cellX = newX;
            cellY = newY;
            GameManager.Instance.BoardManager.SetCellObject(cellX, cellY, this);
            transform.position = GameManager.Instance.BoardManager.CellToWorld(cellX, cellY);
        }
    }
}
