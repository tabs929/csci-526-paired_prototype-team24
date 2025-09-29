using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Min(0)] public int lives = 3;
    public int score = 0;
    bool isGameOver = false;

    public bool debugLogs = false;

    public GameObject gameOverPanel;
    public TMP_Text livesText;
    public TMP_Text scoreText;
    public TMP_Text gameOverTitleText;
    public TMP_Text finalScoreText;

    public GameObject startMenuPanel;
    public TMP_Text startTitleText;
    public TMP_Text rulesText;
    public bool showStartMenuOnLoad = true;

    public bool autoFindUI = true;
    public string livesObjectName = "LivesText";
    public string scoreObjectName = "ScoreText";
    public string gameOverPanelName = "GameOverPanel";
    public string gameOverTitleName = "TitleText";
    public string finalScoreName = "FinalScoreText";
    public string retryButtonName = "RetryButton";
    public string startMenuPanelName = "StartMenuPanel";
    public string startTitleName = "StartTitleText";
    public string rulesObjectName = "RulesText";
    public string startButtonName = "StartButton";

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        UpdateUI();
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        Time.timeScale = 1f;
        isGameOver = false;

        if (autoFindUI)
            RebindUIReferences();

        UpdateUI();

        if (showStartMenuOnLoad)
        {
            TryShowStartMenu();
        }
        else
        {
            if (startMenuPanel) startMenuPanel.SetActive(false);
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
        if (debugLogs) Debug.Log($"Score: {score}");
    }

    public void LoseLife(int amount = 1)
    {
        lives = Mathf.Max(0, lives - amount);
        UpdateUI();
        if (debugLogs) Debug.Log($"Lives: {lives}");
        if (lives <= 0)
        {
            GameOver("Out of lives");
        }
    }

    public void GameOver(string reason = null)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (debugLogs)
        {
            if (!string.IsNullOrEmpty(reason)) Debug.Log($"Game Over: {reason}");
            else Debug.Log("Game Over");
        }

        if (gameOverPanel)
        {
            Time.timeScale = 0f;
            gameOverPanel.SetActive(true);
            if (gameOverTitleText) gameOverTitleText.text = "Game Over";
            if (finalScoreText) finalScoreText.text = $"Score: {score}";

            EnsureEventSystem();
        }
        else
        {
            var idx = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(idx);
        }
    }

    public void CorrectLandingAward()
    {
        AddScore(10);
    }

    public void WrongLandingPenalty()
    {
        LoseLife(1);
    }

    void UpdateUI()
    {
        if (livesText) livesText.text = $"Lives: {lives}";
        if (scoreText) scoreText.text = $"Score: {score}";
    }

    void RebindUIReferences()
    {
        if (!livesText) livesText = FindTMPInScene(livesObjectName);
        if (!scoreText) scoreText = FindTMPInScene(scoreObjectName);
        if (!gameOverPanel) gameOverPanel = FindGOInScene(gameOverPanelName);
        if (!gameOverTitleText) gameOverTitleText = FindTMPInScene(gameOverTitleName);
        if (!finalScoreText) finalScoreText = FindTMPInScene(finalScoreName);

        if (!startMenuPanel) startMenuPanel = FindGOInScene(startMenuPanelName);
        if (!startTitleText) startTitleText = FindTMPInScene(startTitleName);
        if (!rulesText) rulesText = FindTMPInScene(rulesObjectName);

        var retryGO = FindGOInScene(retryButtonName);
        if (retryGO)
        {
            var btn = retryGO.GetComponent<Button>();
            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(Retry);
            }
        }

        var startBtnGO = FindGOInScene(startButtonName);
        if (startBtnGO)
        {
            var btn = startBtnGO.GetComponent<Button>();
            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(StartGame);
            }
        }
    }

    TMP_Text FindTMPInScene(string name)
    {
        var go = FindGOInScene(name);
        return go ? go.GetComponent<TMP_Text>() : null;
    }

    GameObject FindGOInScene(string name)
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var tr = FindChildRecursive(root.transform, name);
            if (tr) return tr.gameObject;
        }
        return null;
    }

    Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            var found = FindChildRecursive(child, name);
            if (found) return found;
        }
        return null;
    }

    void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        lives = 3;
        score = 0;
        UpdateUI();

        var idx = SceneManager.GetActiveScene().buildIndex;
        showStartMenuOnLoad = false;
        SceneManager.LoadScene(idx);
    }

    void TryShowStartMenu()
    {
        if (!startMenuPanel) return;
        startMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        EnsureEventSystem();
        if (startTitleText && string.IsNullOrWhiteSpace(startTitleText.text))
            startTitleText.text = "Color Block Climb";
        if (rulesText && string.IsNullOrWhiteSpace(rulesText.text))
        {
            rulesText.text = "Rules:\n" +
                             "- Up Arrow: Jump\n" +
                             "- A / D: Rotate block\n" +
                             "- Land with the bottom color matching the platform to score +10\n" +
                             "- Mismatch costs 1 life\n" +
                             "- You have 3 lives. Reach as high as you can!";
        }
    }

    public void StartGame()
    {
        if (startMenuPanel) startMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isGameOver = false;
        lives = 3;
        score = 0;
        UpdateUI();
        showStartMenuOnLoad = false;
    }
}
