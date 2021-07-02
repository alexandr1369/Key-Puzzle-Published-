using UnityEngine;

public class RestartButton : InteractableButton
{
    [SerializeField] private CanvasGroup canvasGroup;
    protected override void Perform()
    {
        if(canvasGroup.alpha != 0)
        {
            GameManager.Instance.RestartLevel();
        }
    }
}
