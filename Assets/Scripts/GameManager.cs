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
        if (!Instance)
            Instance = this;
    }

    #endregion

    [Header("UI Panels")]
    [SerializeField] private CanvasGroup victoryPanel;
    [SerializeField] private CanvasGroup loosePanel;

    [Header("Current Scene Locks")]
    [SerializeField] private List<Lock> locks;
    [SerializeField] private int movesAmount;

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

    #region Fields

    private bool isAboutToWin;
    private bool isVictory;
    private bool isLoose;
    private bool isFirstTap;

    private int currentLevel;
    private int currentMovesAmount;

    private Sequence sequence;

    #endregion

    private void Start()
    {
        isAboutToWin = false;
        isVictory = false;
        isLoose = false;
        isFirstTap = false;

        currentMovesAmount = 0;

        currentLevel = PlayerPrefs.GetInt("_keyPuzzleLevel");
        if (currentLevel == 0) currentLevel = 1;
        currentLevelText.text = "Level " + currentLevel;
    }
    private void Update()
    {
        leftMovesAmountText.text = (movesAmount - currentMovesAmount).ToString();

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
                    VibrationManager.Instance.SetHaptic(MoreMountains.NiceVibrations.HapticTypes.Success);
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
                    VibrationManager.Instance.SetHaptic(MoreMountains.NiceVibrations.HapticTypes.Failure);
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
            Victory();
        }
    }
    private IEnumerator CheckForLoose()
    {
        yield return null;
        yield return null;

        bool isStucked = IsPlayerStucked();
        bool isOutOfAvailableMoves = !isVictory && movesAmount <= currentMovesAmount;
        if (!isAboutToWin && (isStucked || isOutOfAvailableMoves))
        {
            Loose();
        }
    }

    #endregion

    #region Main Logic

    public void Restart()
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
    public void IncreaseMovesAmount()
    {
        currentMovesAmount++;
    }
    public void TogglePrewinning(bool state)
    {
        isAboutToWin = state;
    }

    public bool IsLastMove()
    {
        return movesAmount - currentMovesAmount <= 1 && locks.FindAll(t => t.isLocked).Count <= 1;
    }
    public bool CanDoAction()
    {
        return !isVictory && !isLoose;
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
            //keys.ForEach(t => print(t.name));
            allKeys.AddRange(keys);
        }
        for (int i = 0; i < locks.Count; i++)
        {
            Lock currentLock = locks[i];
            allKeysHost = new List<Key>(allKeys);
            if (currentLock.isLocked)
            {
                neededKeysAmount = currentLock.HolesAmount;
                int matchedKeysAmount = allKeysHost.FindAll(t => t.colorType == currentLock.colorType).Count;
                if (neededKeysAmount <= matchedKeysAmount)
                {
                    isStucked = false;
                    break;
                }
            }
        }

        return isStucked;
    }

    #endregion
}