using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Lives")]
    public int startingLives = 3;
    public Sprite heartFull;
    public Sprite heartEmpty;

    [Header("HUD size (the canvas is 1080 x 1920)")]
    public float heartSize = 110f;
    public float heartSpacing = 20f;
    public int fontSize = 60;
    public TMP_FontAsset font;

    [Header("Game over")]
    public float restartDelay = 3f;

    const string BestKey = "BestTime";

    int lives;
    float elapsed;
    float bestTime;
    bool gameOver;

    Transform canvasTransform;
    Image[] hearts;
    TextMeshProUGUI timeText;
    TextMeshProUGUI bestText;
    TextMeshProUGUI gameOverText;

    void OnEnable()  { Egg.EggLost += OnEggLost; }
    void OnDisable() { Egg.EggLost -= OnEggLost; }

    void Start()
    {
        Time.timeScale = 1f;
        lives = startingLives;
        bestTime = PlayerPrefs.GetFloat(BestKey, 0f);
        BuildUI();
        UpdateHearts();
        UpdateBestText();
    }

    void Update()
    {
        if (gameOver) return;
        elapsed += Time.deltaTime;
        timeText.text = "TIME " + FormatTime(elapsed);
    }

    void OnEggLost(Egg egg)
    {
        if (gameOver) return;

        lives--;
        UpdateHearts();

        if (lives <= 0) StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        gameOver = true;

        bool newBest = elapsed > bestTime;
        if (newBest)
        {
            bestTime = elapsed;
            PlayerPrefs.SetFloat(BestKey, bestTime);
            PlayerPrefs.Save();
        }
        UpdateBestText();

        gameOverText.text = "GAME OVER\n" + FormatTime(elapsed) + (newBest ? "\nNEW BEST!" : "");
        gameOverText.gameObject.SetActive(true);

        AudioManager.PlayGameOver();

        Time.timeScale = 0f; // freeze the game
        yield return new WaitForSecondsRealtime(restartDelay);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < lives ? heartFull : heartEmpty;
        }
    }

    void UpdateBestText()
    {
        bestText.text = bestTime > 0f ? "BEST " + FormatTime(bestTime) : "BEST --";
    }

    string FormatTime(float t)
    {
        int minutes = (int)(t / 60f);
        int seconds = (int)(t % 60f);
        return minutes + ":" + seconds.ToString("00");
    }


    void BuildUI()
    {
        GameObject canvasObj = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler));
        Transform parent = canvasObj.transform;
        canvasTransform = parent;

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        // hearts in top left
        hearts = new Image[startingLives];
        for (int i = 0; i < startingLives; i++)
        {
            GameObject h = new GameObject("Heart " + i, typeof(RectTransform), typeof(Image));
            h.transform.SetParent(parent, false);

            Image img = h.GetComponent<Image>();
            img.sprite = heartFull;
            img.preserveAspect = true;

            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(heartSize, heartSize);
            rt.anchoredPosition = new Vector2(30f + i * (heartSize + heartSpacing), -30f);

            hearts[i] = img;
        }

        // time and best time, top right
        Vector2 topRight = new Vector2(1f, 1f);
        timeText = MakeText(parent, "Time Text", topRight, new Vector2(-30f, -30f), TextAlignmentOptions.TopRight);
        bestText = MakeText(parent, "Best Text", topRight, new Vector2(-30f, -30f - fontSize * 1.3f), TextAlignmentOptions.TopRight);

        // game over message, center
        gameOverText = MakeText(parent, "Game Over Text", new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center);
        gameOverText.fontSize = fontSize * 2;
        gameOverText.rectTransform.sizeDelta = new Vector2(1000f, fontSize * 6f);
        gameOverText.gameObject.SetActive(false);
    }

    TextMeshProUGUI MakeText(Transform parent, string objName, Vector2 anchor, Vector2 position, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(objName, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.color = Color.white;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;

        // dark outline for readable text
        t.outlineWidth = 0.25f;
        t.outlineColor = Color.black;

        RectTransform rt = t.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.sizeDelta = new Vector2(700f, fontSize * 1.3f);
        rt.anchoredPosition = position;

        return t;
    }
}