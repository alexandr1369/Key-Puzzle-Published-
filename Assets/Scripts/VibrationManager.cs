using UnityEngine;
using MoreMountains.NiceVibrations;

public class VibrationManager : MonoBehaviour
{
    #region Singleton

    public static VibrationManager Instance;
    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }

    #endregion

    private bool isHapticSupported;

    private void Start()
    {
        isHapticSupported = MMVibrationManager.HapticsSupported();
        MMVibrationManager.SetHapticsActive(isHapticSupported);
    }

    public void SetHaptic(HapticTypes type)
    {
        MMVibrationManager.Haptic(type, true, true, this);
    }
}
