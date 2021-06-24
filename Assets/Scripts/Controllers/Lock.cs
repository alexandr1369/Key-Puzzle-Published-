using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[ExecuteInEditMode]
public class Lock : MonoBehaviour
{
    [HideInInspector]
    public bool isLocked;

    [Header("Settings")]
    public ColorType colorType = ColorType.Blue;
    public Key slotKey;

    [Header("Others")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private List<LockHole> holes;

    [Header("Colored Parts")]
    [SerializeField] List<MeshRenderer> parts;

    #region Fields
    public bool IsSlotKeySelected { get; set; }
    public int HolesAmount { get => holes.Count; }

    private Key selectedKey;
    private Sequence sequence;
    private bool isAboutToBeUnlocked;
    private float openingAnimDuration = .4f;
    private float selectionAnimDuration = .4f;

    #endregion

    private void Start()
    {
        isLocked = true;
        isAboutToBeUnlocked = false;

        if (slotKey)
        {
            slotKey.IsFree = false;
        }

        for(int i = 0; i < holes.Count; i++)
        {
            Key key = holes[i].Key;
            if (key)
            {
                key.IsFree = true;
            }
        }
    }
    private void Update()
    {
#if UNITY_EDITOR
        if(parts != null)
        {
            Material material = Resources.Load<Material>("Materials/" + colorType.ToString() + " Color Material");
            parts.ForEach(t => t.material = material);
        }
#endif
    }

    #region Main Logic

    public Key GetSelectedKey()
    {
        selectedKey = null;
        if (isLocked)
        {
            if (holes.Count > 0)
            {
                for (int i = holes.Count - 1; i >= 0; i--)
                {
                    Key key = holes[i].Key;
                    if (key)
                    {
                        selectedKey = key;
                        break;
                    }
                }
            }
        }
        else
        {
            if (slotKey)
            {
                selectedKey = slotKey;
            }
        }

        return selectedKey;
    }
    public LockHole GetLockHole(Key key)
    {
        LockHole lockHole;
        if (holes.Count > 0)
        {
            if (key)
            {
                for(int i = holes.Count - 1; i >= 0; i--)
                {
                    lockHole = holes[i];
                    if (lockHole.Key == key)
                    {
                        return lockHole;
                    }
                }
            }
            else
            {
                for (int i = 0; i < holes.Count; i++)
                {
                    lockHole = holes[i];
                    if (!lockHole.Key)
                    {
                        return lockHole;
                    }
                }
            }
        }

        return null;
    }
    public List<Key> GetFreeKeys()
    {
        List<Key> keys = new List<Key>();
        if(slotKey && slotKey.IsFree)
        {
            keys.Add(slotKey);
        }
        else
        {
            foreach (LockHole hole in holes)
            {
                if (hole.Key && hole.Key.IsFree)
                {
                    keys.Add(hole.Key);
                }
            }
        }

        return keys;
    }
    private int GetAmountOfEmptyLockHoles()
    {
        int amount = 0;
        foreach(LockHole lockHole in holes)
        {
            if (!lockHole.Key)
            {
                amount++;
            }
        }

        return amount;
    }

    public void AddSelectedKey(Key newKey)
    {
        if (holes.Count > 0)
        {
            for (int i = 0; i < holes.Count; i++)
            {
                Key key = holes[i].Key;
                if (!key)
                {
                    holes[i].Key = newKey;
                    break;
                }
            }
        }
    }
    public void RemoveSelectedKey()
    {
        if (!selectedKey.IsFree)
        {
            if(slotKey && selectedKey == slotKey)
            {
                slotKey = null;
            }
        }
        else
        {
            if (holes.Count > 0)
            {
                for (int i = holes.Count - 1; i >= 0; i--)
                {
                    Key key = holes[i].Key;
                    if (key && selectedKey == key)
                    {
                        key = null;
                    }

                    holes[i].Key = key;
                }
            }
        }
    }

    #endregion

    #region Animations
    public void OpeningLockWithSelectedLockAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        Lock selectedLock = Player.Instance.SelectedLock;
        Key currentKey = selectedLock.GetSelectedKey();
        bool isKeyRelated = currentKey == selectedKey;
        bool shouldLockBeOpened = !isKeyRelated && GetAmountOfEmptyLockHoles() < 2;
        LockHole lockHole = GetLockHole(isKeyRelated ? currentKey : null);
        Vector3 positionValue = new Vector3(lockHole.transform.position.x, lockHole.transform.position.y, lockHole.transform.position.z - 1f);
        Vector3 headRotationValue = new Vector3(0, 360f, 0);
        Vector3 keyRotationValue1 = new Vector3(180f, 180f, -180f);
        Quaternion keyRotationValue2 = new Quaternion();
        keyRotationValue2.eulerAngles = new Vector3(keyRotationValue1.x, keyRotationValue1.y, 360f);
        sequence
            .OnStart(() =>
            {
                if (GameManager.Instance.IsLastMove())
                {
                    isAboutToBeUnlocked = true;
                    GameManager.Instance.TogglePrewinning(true);
                }

                GameManager.Instance.IncreaseMovesAmount();
                Vector3 lockHolePosition = lockHole.transform.position;
                currentKey.StartPosition = positionValue;
                currentKey.StartRotation = keyRotationValue2.eulerAngles;
                Player.Instance.SelectedLock.RemoveSelectedKey();
                Player.Instance.SelectedLock = null;
                selectedLock.selectedKey = null;
                AddSelectedKey(currentKey);

                if (shouldLockBeOpened)
                {
                    foreach(LockHole hole in holes)
                    {
                        if (hole.Key)
                        {
                            hole.Key.IsFree = false;
                        }
                    }
                    if (slotKey)
                    {
                        slotKey.IsFree = true;
                    }

                    isLocked = false;
                    VibrationManager.Instance.SetHaptic(MoreMountains.NiceVibrations.HapticTypes.MediumImpact);
                }
                else
                {
                    VibrationManager.Instance.SetHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);
                }
            })
            .Append(currentKey.transform.DOMove(positionValue, openingAnimDuration))
            .Join(currentKey.transform.DORotate(keyRotationValue1, openingAnimDuration))
            .Append(currentKey.transform.DORotateQuaternion(keyRotationValue2, openingAnimDuration * 1.5f));

        if(currentKey == selectedLock.slotKey)
        {
            selectedLock.slotKey = null;
        }
        if (shouldLockBeOpened)
        {
            sequence
                .Append(headTransform.DORotate(headRotationValue, openingAnimDuration, RotateMode.Fast))
                .OnComplete(() =>
                {
                    if (GameManager.Instance.IsLastMove() && isAboutToBeUnlocked)
                    {
                        GameManager.Instance.TogglePrewinning(false);
                    }
                });
        }
    }
    public void KeySelectionAnimation()
    {
        if (GetSelectedKey())
        {
            sequence = DOTween.Sequence();
            Vector3 positionValue = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z - 3f);
            Vector3 rotationValue = new Vector3(90f, 180f, -90f);
            sequence
                .OnStart(() =>
                {
                    Player.Instance.SelectedLock = this;
                    selectedKey.ToggleKinematic(true);
                })
                .Append(selectedKey.transform.DORotate(rotationValue, selectionAnimDuration, RotateMode.Fast))
                .Join(selectedKey.transform.DOMove(positionValue, selectionAnimDuration));
        }
    }
    public void KeyDeselectionAnimation()
    {
        sequence = DOTween.Sequence();
        sequence
        .OnStart(() =>
        {
            Player.Instance.SelectedLock = null;
            if(selectedKey == slotKey)
                selectedKey.ToggleKinematic(false);
        })
        .Append(selectedKey.transform.DORotate(selectedKey.StartRotation, selectionAnimDuration, RotateMode.Fast))
        .Join(selectedKey.transform.DOMove(selectedKey.StartPosition, selectionAnimDuration));
     
    }

    #endregion
}