using UnityEngine;
using GameAnalyticsSDK;

public class GAManager : MonoBehaviour
{
    #region Singleton

    public static GAManager Instance;
    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(this);
    }

    #endregion

    private void Start()
    {
        GameAnalytics.Initialize();
    }

    public void OnLevelCompleted(int currentLevel)
    {
        print("Level Completed!");
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level " + currentLevel.ToString());

    }
    public void OnLevelFailed(int currentLevel)
    {
        print($"Level {currentLevel} Failed...");
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "Level " + currentLevel.ToString());
    }
}