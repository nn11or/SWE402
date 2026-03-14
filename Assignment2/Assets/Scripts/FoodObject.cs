using UnityEngine;

public class FoodObject : CellObject
{
    [SerializeField, Range(1, 50), Tooltip("Amount of food granted on pickup")]
    private int amountGranted = 10;

    public override void PlayerEntered()
    {
        GameManager.Instance.ChangeFood(amountGranted);
        AudioManager.Instance?.PlayFoodPickup();
        VFXManager.Instance?.PlayFoodCollect(transform.position);

        GameManager.Instance.BoardManager.SetCellObject(cellX, cellY, null);

        ObjectPool pool = FindFirstObjectByType<ObjectPool>();
        if (pool != null)
            pool.ReturnToPool(this);
        else
            Destroy(gameObject);
    }
}
