using UnityEngine;

public class NoMoves : MonoBehaviour
{
    public void Execute()
    {
        GameManager.Instance.Loose();
    }
}
