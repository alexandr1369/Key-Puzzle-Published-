using UnityEngine;

public class ContinueButton : InteractableButton
{
    [SerializeField] private CanvasGroup canvasGroup;
    protected override void Perform()
    {
        if(canvasGroup.alpha != 0)
        {
            GameManager.Instance.Restart();
        }
    }
}