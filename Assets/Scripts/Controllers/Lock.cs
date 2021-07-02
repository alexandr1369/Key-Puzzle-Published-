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

    [Header("Slot Key")]
    public Key slotKey;

    [Header("Others")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private ParticleSystem threeStepsLeftEffect;
    [SerializeField] private ParticleSystem openEffect;
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
        //if(slotKey == null && slotKeyColor != ColorType.None)
        //{
        //    Transform parentTransform = GameObject.Find("Location/Keys").transform;
        //    Vector3 keyPosition = transform.position + new Vector3(.3f, .75f, 0);
        //    Key slotKeyPrefab = Resources.Load<Key>("Prefabs/Keys/Key" + holes.Count);
        //    Key newKey = Instantiate(slotKeyPrefab, keyPosition, Quaternion.identity, parentTransform);
        //    Quaternion keyRotation = new Quaternion();
        //    keyRotation.eulerAngles = new Vector3(15f, 165f, -90f);
        //    newKey.transform.rotation = keyRotation;
        //    newKey.colorType = slotKeyColor;
        //    newKey.name = newKey.colorType + " Key";
        //    slotKey = newKey;
        //}
        //else if(slotKey.colorType != slotKeyColor)
        //{
        //    slotKey.colorType = slotKeyColor;
        //}

        if (parts != null)
        {
            Material material = Resources.Load<Material>("Materials/" + colorType.ToString() + " Color Material");
            parts.ForEach(t => t.material = material);
            name = colorType.ToString() + " Lock";
        }
#endif
    }

    #region Main Logic

    public bool AreAllKeysTheSameBaseColor()
    {
        if(holes.Count > 0)
        {
            foreach(LockHole lockHole in holes)
            {
                if(lockHole.Key)
                {
                    if (lockHole.Key.colorType == colorType)
                    {
                        continue;
                    }
                    else
                    {
                        return false;     
                    }
                }
            }
        }

        return true;
    }
    public bool HasFreeHoles()
    {
        if(holes.Find(t => !t.Key))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool LastLockHoleKeyColorTypeEqualsLockColorType()
    {
        LockHole lockHole;
        if (holes.Count > 0)
        {
            if(holes.Find(t => t.Key))
            {
                for (int i = holes.Count - 1; i >= 0; i--)
                {
                    lockHole = holes[i];
                    if (lockHole.Key && lockHole.Key.colorType == colorType)
                    {
                        return true;
                    }
                }
            }
            else
            {
                return true;
            }
        }

        return false;
    }
    public int GetAmountOfEmptyLockHoles()
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
    public void ToggleOutline(bool state)
    {
        if (slotKey)
        {
            slotKey.ToggleOutline(state);
        }

        foreach(LockHole lockHole in holes)
        {
            if (lockHole.Key)
            {
                lockHole.Key.ToggleOutline(state);
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
        Vector3 headRotationValue = new Vector3(90, 0, 360f);
        Vector3 keyRotationValue1 = new Vector3(180f, 180f, -180f);
        Quaternion keyRotationValue2 = new Quaternion();
        keyRotationValue2.eulerAngles = new Vector3(keyRotationValue1.x, keyRotationValue1.y, 360f);
        sequence
            .OnStart(() =>
            {
                if (GameManager.Instance.IsLastLockToOpen())
                {
                    isAboutToBeUnlocked = true;
                    GameManager.Instance.TogglePrewinning(true);
                }

                VibrationManager.Instance.SetHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);
                Vector3 lockHolePosition = lockHole.transform.position;
                currentKey.StartPosition = positionValue;
                currentKey.StartRotation = keyRotationValue2.eulerAngles;
                Player.Instance.SelectedLock.RemoveSelectedKey();
                Player.Instance.SelectedLock = null;
                selectedLock.selectedKey = null;
                AddSelectedKey(currentKey);

                if (shouldLockBeOpened)
                {
                    ToggleEffect(false);
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
                .AppendCallback(() =>
                {
                    openEffect.Play();
                    VibrationManager.Instance.SetHaptic(MoreMountains.NiceVibrations.HapticTypes.MediumImpact);
                })
                .Append(headTransform.DORotate(headRotationValue, openingAnimDuration, RotateMode.Fast))
                .OnComplete(() =>
                {
                    
                    if (isAboutToBeUnlocked)
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

    public void ToggleEffect(bool state)
    {
        if (!threeStepsLeftEffect) return;

        if (state)
        {
            if (!threeStepsLeftEffect.isPlaying)
            {
                threeStepsLeftEffect.Simulate(1.75f);
                threeStepsLeftEffect.Play(true);
            }
        }
        else
        {
            if (!threeStepsLeftEffect.isStopped)
            {
                threeStepsLeftEffect.Stop();
            }
        }
    }

    #endregion
}