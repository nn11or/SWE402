using UnityEngine;

public class ExitCellObject : CellObject
{
    public override void PlayerEntered()
    {
        GameManager.Instance.NextLevel();
    }
}
