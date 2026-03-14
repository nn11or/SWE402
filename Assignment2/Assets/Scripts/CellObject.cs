using UnityEngine;

public class CellObject : MonoBehaviour
{
    [SerializeField, Tooltip("Prefab identifier for object pooling")]
    private string prefabId;

    protected int cellX;
    protected int cellY;

    public string PrefabId => prefabId;

    public virtual void Init(int x, int y)
    {
        cellX = x;
        cellY = y;
    }

    public virtual bool PlayerWantsToEnter()
    {
        return true;
    }

    public virtual void PlayerEntered()
    {
    }

    public void SetPrefabId(string id)
    {
        prefabId = id;
    }
}
