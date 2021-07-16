using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager Instance;
    private void Awake()
    {
        Instance = this;

        //currentLevel = PlayerPrefs.GetInt("_keyPuzzleLevel");
        //if (currentLevel == 0)
        //{
        //    currentLevel = 1;
        //    PlayerPrefs.SetInt("_keyPuzzleLevel", currentLevel);
        //}
        //else if (currentLevel - 1 != SceneManager.GetActiveScene().buildIndex)
        //{
        //    //print(currentLevel + " " + SceneManager.GetActiveScene().buildIndex);
        //    LoadNextLevel(currentLevel);
        //}
    }

    #endregion
    
    [Header("UI Panels")]
    [SerializeField] private CanvasGroup victoryPanel;
    [SerializeField] private CanvasGroup loosePanel;

    [Header("Current Scene Locks")]
    [SerializeField] private List<Lock> locks;

    [Header("Game Begin")]
    [SerializeField] private CanvasGroup startInfoCG;
    [SerializeField] private CanvasGroup infoPanelCG;
    [SerializeField] private Text currentLevelText;
    [SerializeField] private Text leftMovesAmountText;

    [Header("Game Over")]
    [SerializeField] private SpriteRenderer foregroundSR;
    [SerializeField] private ParticleSystem victoryPS;
    [SerializeField] private ParticleSystem victorySmilePS;
    [SerializeField] private ParticleSystem looseSmilePS;
    [SerializeField] private Animator noMovesAnimator;

    #region Fields

    private bool isAboutToLoose;
    private bool isAboutToWin;
    private bool isVictory;
    private bool isLoose;
    private bool isFirstTap;

    private int currentLevel;

    private Sequence sequence;

    #endregion

    private void Start()
    {
        //PlayerPrefs.SetInt("_keyPuzzleLevel", 1);

        isAboutToLoose = false;
        isAboutToWin = false;
        isVictory = false;
        isLoose = false;
        isFirstTap = false;
    }
    private void Update()
    {
        CheckForThreeLocksLeft();

        StartCoroutine(CheckForVictory());
        StartCoroutine(CheckForLoose());
    }

    #region Game Over

    public void Victory()
    {
        if (!isVictory)
        {
            isVictory = true;
            sequence = DOTween.Sequence();
            sequence
                .Append(DOTween.To(t => victoryPanel.GetComponent<CanvasGroup>().alpha = t, 0, 1f, 2f))
                .Join(DOTween.To(t => foregroundSR.color = new Color(foregroundSR.color.r, foregroundSR.color.g, foregroundSR.color.b, t), 0, 100 / 255f, 2f))
                .Join(DOTween.To(t => infoPanelCG.alpha = t, 1, .65f, 2f))
                .SetEase(Ease.OutBack)
                .OnStart(() =>
                {
                    GAManager.Instance.OnLevelCompleted(currentLevel);
                    PlayerPrefs.SetInt("_keyPuzzleLevel", ++currentLevel);
                    VibrationManager.Instance.SetHaptic(MoreMountains.NiceVibrations.HapticTypes.Success);
                    locks.ForEach(t => t.ToggleOutline(false));
                    victoryPanel.blocksRaycasts = true;
                    loosePanel.blocksRaycasts = false;
                    victorySmilePS.Play();
                    victoryPS.Play();
                });
        }
    }
    public void Loose()
    {
        if (!isLoose && !isVictory)
        {
            isLoose = true;
            sequence = DOTween.Sequence();
            sequence
                .Append(DOTween.To(t => loosePanel.GetComponent<CanvasGroup>().alpha = t, 0, 1f, 2f))
                .Join(DOTween.To(t => foregroundSR.color = new Color(foregroundSR.color.r, foregroundSR.color.g, foregroundSR.color.b, t), 0, 100 / 255f, 2f))
                .Join(DOTween.To(t => infoPanelCG.alpha = t, 1, .65f, 2f))
                .SetEase(Ease.OutBack)
                .OnStart(() =>
                {
                    GAManager.Instance.OnLevelFailed(currentLevel);
                    VibrationManager.Instance.SetHaptic(MoreMountains.NiceVibrations.HapticTypes.Failure);
                    locks.ForEach(t => t.ToggleOutline(false));
                    victoryPanel.blocksRaycasts = false;
                    loosePanel.blocksRaycasts = true;
                    looseSmilePS.Play();
                });
        }
    }
    private IEnumerator CheckForVictory()
    {
        yield return null;

        bool hasVictory = true;
        foreach (Lock targetLock in locks)
        {
            if (targetLock.isLocked)
            {
                hasVictory = false;
                continue;
            }
        }
        if (!isAboutToWin && hasVictory)
        {
            isAboutToWin = true;
            yield return new WaitForSeconds(.25f);
            Victory();
        }
    }
    private IEnumerator CheckForLoose()
    {
        yield return null;
        yield return null;

        bool isStucked = IsPlayerStucked();
        //bool isOutOfAvailableMoves = !isVictory && movesAmount <= currentMovesAmount;
        if (!isAboutToWin && (isStucked/* || isOutOfAvailableMoves*/))
        {
            isAboutToLoose = true;
            noMovesAnimator.SetBool("Appearing", true);
            //yield return new WaitForSeconds(1f);
            //Loose();
        }
    }

    #endregion

    #region Main Logic

    public void LoadNextLevel(int? levelIndex)
    {
        DOTween.Clear(true);
        int nextSceneIndex;
        if (levelIndex != null)
        {
            nextSceneIndex = levelIndex.Value - 1;
            if (nextSceneIndex > /*5*/14)
            {
                nextSceneIndex = 0;
                 PlayerPrefs.SetInt("_keyPuzzleLevel", 1);

            }
        }
        else
        {
            nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex > /*5*/14)
            {
                nextSceneIndex = 0;
                PlayerPrefs.SetInt("_keyPuzzleLevel", 1);
            }
        }
        SceneManager.LoadScene(nextSceneIndex);
    }
    public void RestartLevel()
    {
        DOTween.Clear(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void FirstTapAction()
    {
        if (!isFirstTap)
        {
            float currentAlpha = startInfoCG.alpha;
            DOTween
                .To(t => startInfoCG.alpha = t, currentAlpha, 0, .15f)
                .OnStart(() => isFirstTap = false);
        }
    }
    public void TogglePrewinning(bool state)
    {
        isAboutToWin = state;
    }
    private void CheckForThreeLocksLeft()
    {
        if (locks.Count <= 3) return;

        List<Lock> hostLocks = locks.FindAll(t => t.isLocked);
        if (hostLocks.Count <= 3)
        {
            hostLocks.ForEach(t => t.ToggleEffect(true));
        }
    }
    public bool IsLastLockToOpen()
    {
        if(locks.FindAll(t => t.isLocked).Count == 1)
        {
            if(locks.Find(t => t.isLocked).GetAmountOfEmptyLockHoles() == 1)
            {
                return true;
            }
        }
        return false;
    }
    public bool CanDoAction()
    {
        return !isVictory && !isLoose && !isAboutToWin && !isAboutToLoose;
    }
    private bool IsPlayerStucked()
    {
        int neededKeysAmount;
        bool isStucked = true;
        List<Key> allKeys = new List<Key>();
        List<Key> allKeysHost = new List<Key>();
        for (int i = 0; i < locks.Count; i++)
        {
            List<Key> keys = locks[i].GetFreeKeys();
            allKeys.AddRange(keys);
        }
        for (int i = 0; i < locks.Count; i++)
        {
            Lock currentLock = locks[i];
            allKeysHost = new List<Key>(allKeys);
            if (currentLock.isLocked)
            {
                neededKeysAmount = currentLock.HolesAmount;
                int matchedKeysAmount = allKeysHost.FindAll(t => t.colorType == currentLock.colorType && (currentLock.HasFreeHoles() || CanLockFreeHoles(currentLock))).Count;
                if (neededKeysAmount <= matchedKeysAmount)
                {
                    isStucked = false;
                    break;
                }
            }
        }

        return isStucked;
    }
    private bool CanLockFreeHoles(Lock currentLock)
    {
        // DEMO
        // может ли текущий какой-то замок переместить куда-то свой последний ключ
        bool state = false;
        List<Lock> hostLocks = locks.FindAll(t => t != currentLock);
        foreach(Lock hostLock in hostLocks)
        {
            // замок должен быть закрыт
            if (hostLock.isLocked)
            {
                // замок должен иметь уже все вставленные ключи его цвета
                // или пусто или все уже одного цвета - можно добавлять новые 
                // &&
                // цвет ключа для выбора должен совпадать с замком
                if (hostLock.AreAllKeysTheSameBaseColor() && currentLock.GetSelectedKey().colorType == hostLock.colorType)
                {   
                    // замок должен иметь минимум еще одну свободную щель
                    if (hostLock.HasFreeHoles())
                    {
                        state = true;
                        //print(state == true);
                        break;
                    }
                }
            }
        }

        return state;
    }

    #endregion
}
