using UnityEngine;

public class Player : MonoBehaviour
{
    #region Singleton

    public static Player Instance;
    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }

    #endregion
    
    public Lock SelectedLock { get; set; }

    private void Update()
    {
        Lock targetLock;
        RaycastHit[] hits;
        LayerMask layerMask = 1 << LayerMask.NameToLayer("Lock");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hits = Physics.RaycastAll(ray, 20, layerMask);
        foreach (RaycastHit hit in hits)
        {
            targetLock = hit.collider.GetComponent<Lock>();
            if (GameManager.Instance.CanDoAction() && targetLock)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (targetLock.isLocked)
                    {
                        if (SelectedLock || targetLock.GetSelectedKey())
                        {
                            if (SelectedLock == targetLock)
                            {
                                SelectedLock.KeyDeselectionAnimation();
                            }
                            else
                            {
                                if (CanOpenLock(targetLock))
                                {
                                    targetLock.OpeningLockWithSelectedLockAnimation();
                                }
                                else
                                {
                                    SelectNewKey(targetLock);
                                    SelectedLock.KeySelectionAnimation();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (targetLock.slotKey)
                        {
                            if (!targetLock.IsSlotKeySelected)
                            {
                                SelectNewKey(targetLock);
                                SelectedLock.KeySelectionAnimation();
                            }
                        }
                    }
                }
            }
        }

        // check for first tap anywhere
        if (Input.GetMouseButtonDown(0))
        {
            GameManager.Instance.FirstTapAction();
        }
    }

    public void SelectNewKey(Lock targetLock)
    {
        if (SelectedLock)
        {
            SelectedLock.KeyDeselectionAnimation();
        }

        SelectedLock = targetLock;
    }
    public bool CanOpenLock(Lock targetLock)
    {
        return SelectedLock && targetLock.GetLockHole(null) && SelectedLock.GetSelectedKey().colorType == targetLock.colorType;
    }
}
