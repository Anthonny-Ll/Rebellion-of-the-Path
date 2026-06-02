using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Objetivo")]
    public int enemiesRequired = 1;
    private int enemiesKilled = 0;

    TextMeshProUGUI killCounterText;
    GameObject victoryPanel;
    GameObject gameOverPanel;
    bool gameEnded = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Asegurar EventSystem para clicks
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        Invoke(nameof(BuildGameUI), 0.3f);
    }

    void BuildGameUI()
    {
        // Canvas PROPIO con GraphicRaycaster — clave para que funcionen los botones
        GameObject canvasGO = new GameObject("GameManagerCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Contador de enemigos
        GameObject counterGO = new GameObject("KillCounter");
        counterGO.transform.SetParent(canvasGO.transform, false);
        RectTransform counterRT = counterGO.AddComponent<RectTransform>();
        counterRT.anchorMin = new Vector2(0.5f, 1f);
        counterRT.anchorMax = new Vector2(0.5f, 1f);
        counterRT.pivot = new Vector2(0.5f, 1f);
        counterRT.anchoredPosition = new Vector2(0, -15f);
        counterRT.sizeDelta = new Vector2(400, 50);
        killCounterText = counterGO.AddComponent<TextMeshProUGUI>();
        killCounterText.fontSize = 30;
        killCounterText.color = Color.white;
        killCounterText.alignment = TextAlignmentOptions.Center;
        killCounterText.fontStyle = FontStyles.Bold;

        // Paneles
        victoryPanel = CreateEndPanel(canvasGO, "VictoryPanel",
            new Color(0f, 0.9f, 0.3f), "¡VICTORIA!",
            "Aethelgard está a salvo... por ahora.");
        victoryPanel.SetActive(false);

        gameOverPanel = CreateEndPanel(canvasGO, "GameOverPanel",
            new Color(0.9f, 0.1f, 0.1f), "GAME OVER",
            "La Falange ha ganado esta batalla.");
        gameOverPanel.SetActive(false);

        UpdateKillCounter();
        Debug.Log("GameManager UI creada.");
    }

    GameObject CreateEndPanel(GameObject canvas, string name,
                               Color titleColor, string title, string subtitle)
    {
        // Fondo
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.78f);
        img.raycastTarget = true;

        // Título
        MakeText(panel, "Title", new Vector2(0.5f, 0.58f), new Vector2(700, 90),
                 title, 65, titleColor, FontStyles.Bold);

        // Subtítulo
        MakeText(panel, "Sub", new Vector2(0.5f, 0.47f), new Vector2(600, 45),
                 subtitle, 24, Color.white, FontStyles.Normal);

        // Botón
        GameObject btn = new GameObject("RetryBtn");
        btn.transform.SetParent(panel.transform, false);
        RectTransform btnRT = btn.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0.36f);
        btnRT.anchorMax = new Vector2(0.5f, 0.36f);
        btnRT.pivot = new Vector2(0.5f, 0.5f);
        btnRT.anchoredPosition = Vector2.zero;
        btnRT.sizeDelta = new Vector2(240, 55);
        Image btnImg = btn.AddComponent<Image>();
        btnImg.color = new Color(0.15f, 0.15f, 0.3f);
        btnImg.raycastTarget = true;
        Button btnComp = btn.AddComponent<Button>();
        ColorBlock cb = btnComp.colors;
        cb.highlightedColor = new Color(0.3f, 0.3f, 0.55f);
        cb.pressedColor     = new Color(0.05f, 0.05f, 0.15f);
        btnComp.colors = cb;
        btnComp.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });

        MakeText(btn, "BtnText", new Vector2(0.5f, 0.5f), new Vector2(220, 50),
                 "REINTENTAR", 22, Color.white, FontStyles.Bold);

        return panel;
    }

    void MakeText(GameObject parent, string goName, Vector2 anchor,
                  Vector2 size, string text, float fontSize,
                  Color color, FontStyles style)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text       = text;
        tmp.fontSize   = fontSize;
        tmp.color      = color;
        tmp.fontStyle  = style;
        tmp.alignment  = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    public void EnemyKilled()
    {
        if (gameEnded) return;
        enemiesKilled++;
        Debug.Log($"GameManager: Enemigo eliminado {enemiesKilled}/{enemiesRequired}");
        UpdateKillCounter();
        if (enemiesKilled >= enemiesRequired)
            TriggerVictory();
    }

    public void TriggerGameOver()
    {
        if (gameEnded) return;
        gameEnded = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("GameManager: GAME OVER");
    }

    void TriggerVictory()
    {
        gameEnded = true;
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("GameManager: VICTORIA");
    }

    void UpdateKillCounter()
    {
        if (killCounterText != null)
            killCounterText.text = $"ENEMIGOS: {enemiesKilled} / {enemiesRequired}";
    }
}
