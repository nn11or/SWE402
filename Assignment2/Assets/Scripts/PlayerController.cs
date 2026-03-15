using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField, Range(1f, 20f), Tooltip("Speed of smooth movement between cells")]
    private float moveSpeed = 4f;

    private int cellX;
    private int cellY;
    private bool isMoving;
    private bool inputEnabled = true;
    private Animator animator;

    public int CellX => cellX;
    public int CellY => cellY;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!inputEnabled || isMoving) return;

        int moveX = 0;
        int moveY = 0;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            moveY = 1;
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            moveY = -1;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            moveX = -1;
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            moveX = 1;

        if (moveX == 0 && moveY == 0) return;

        TryMove(moveX, moveY);
    }

    private void TryMove(int dx, int dy)
    {
        int newX = cellX + dx;
        int newY = cellY + dy;

        BoardManager.CellData cellData = GameManager.Instance.BoardManager.GetCellData(newX, newY);

        if (cellData == null || !cellData.Passable)
            return;

        if (cellData.ContainedObject != null)
        {
            bool canEnter = cellData.ContainedObject.PlayerWantsToEnter();

            if (!canEnter)
            {
                if (animator != null)
                    animator.SetTrigger("Attack");

                GameManager.Instance.TurnManager.Tick();
                GameManager.Instance.ChangeFood(-1);
                return;
            }
        }

        // Flip sprite based on direction
        if (dx != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = dx < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        Vector3 targetPos = GameManager.Instance.BoardManager.CellToWorld(newX, newY);
        StartCoroutine(SmoothMove(targetPos));

        cellX = newX;
        cellY = newY;

        if (cellData.ContainedObject != null)
            cellData.ContainedObject.PlayerEntered();

        AudioManager.Instance?.PlayPlayerMove();

        if (animator != null)
            animator.SetBool("IsWalking", true);

        GameManager.Instance.TurnManager.Tick();
        GameManager.Instance.ChangeFood(-1);
    }

    private IEnumerator SmoothMove(Vector3 targetPos)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        float duration = 1f / moveSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;

        if (animator != null)
            animator.SetBool("IsWalking", false);
    }

    public void Spawn(int x, int y)
    {
        cellX = x;
        cellY = y;
        transform.position = GameManager.Instance.BoardManager.CellToWorld(x, y);
    }

    public void EnableInput(bool enable)
    {
        inputEnabled = enable;
    }
}
