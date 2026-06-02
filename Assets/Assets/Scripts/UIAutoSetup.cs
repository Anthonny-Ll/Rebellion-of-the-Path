using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Crea toda la UI automaticamente al iniciar el juego.
/// Solo agrega este script a cualquier GameObject en la escena.
/// NO necesitas crear Canvas, Sliders ni Textos manualmente.
/// </summary>
public class UIAutoSetup : MonoBehaviour
{
    // Referencias que se crean automaticamente
    Slider healthBar;
    Slider energyBar;
    TextMeshProUGUI scoreText;
    TextMeshProUGUI comboText;
    TextMeshProUGUI levelText;

    // Referencias al jugador
    PlayerController player;
    DaggerController dagger;

    int score = 0;
    int combo = 1;
    int kills = 0;
    float comboTimer = 0;
    const float COMBO_TIME = 3f;

    void Awake()
    {
        BuildUI();
    }

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        dagger = FindFirstObjectByType<DaggerController>();
    }

    // ── CONSTRUYE TODA LA UI DESDE CODIGO ───────────────
    void BuildUI()
    {
        // 1. Canvas
        GameObject canvasGO = new GameObject("GameCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        // 2. Panel izquierdo — vida y energia
        GameObject leftPanel = MakePanel(canvasGO, "LeftPanel",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(10, -10), new Vector2(220, 80));

        // Barra de vida
        healthBar = MakeSlider(leftPanel, "HealthBar",
            new Vector2(0, 52), new Vector2(200, 20),
            new Color(0.12f, 0.87f, 0.37f));

        // Label HP
        MakeLabel(leftPanel, "HP", new Vector2(0, 64),
            new Vector2(30, 18), new Color(0.12f, 0.87f, 0.37f), 12);

        // Barra de energia
        energyBar = MakeSlider(leftPanel, "EnergyBar",
            new Vector2(0, 28), new Vector2(200, 14),
            new Color(0f, 1f, 0.96f));

        // Label EN
        MakeLabel(leftPanel, "EN", new Vector2(0, 38),
            new Vector2(30, 16), new Color(0f, 1f, 0.96f), 11);

        // 3. Panel derecho — puntuacion y combo
        GameObject rightPanel = MakePanel(canvasGO, "RightPanel",
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-10, -10), new Vector2(220, 90));

        scoreText = MakeTMP(rightPanel, "ScoreText",
            new Vector2(-10, -20), new Vector2(200, 35),
            "PUNTOS: 0", 24, Color.white, TextAlignmentOptions.Right);

        comboText = MakeTMP(rightPanel, "ComboText",
            new Vector2(-10, -55), new Vector2(200, 30),
            "", 20, new Color(1f, 0.2f, 0.2f), TextAlignmentOptions.Right);
        comboText.gameObject.SetActive(false);

        levelText = MakeTMP(rightPanel, "LevelText",
            new Vector2(-10, -80), new Vector2(200, 25),
            "NIVEL 1", 16, new Color(0.6f, 0.6f, 1f), TextAlignmentOptions.Right);

        // Texto de distrito eliminado para no interferir con el contador de enemigos

        Debug.Log("UI creada automaticamente.");
    }

    // ── UPDATE — actualiza barras y textos ───────────────
    void Update()
    {
        UpdateBars();
        UpdateCombo();
    }

    void UpdateBars()
    {
        if (player != null && healthBar != null)
        {
            float pct = (float)player.currentHealth / player.maxHealth;
            healthBar.value = Mathf.Lerp(healthBar.value, pct, Time.deltaTime * 8f);

            // Color segun vida
            var fill = healthBar.fillRect.GetComponent<Image>();
            if (fill != null)
                fill.color = pct > 0.6f
                    ? new Color(0.12f, 0.87f, 0.37f)
                    : pct > 0.3f
                        ? new Color(1f, 0.8f, 0f)
                        : new Color(0.9f, 0.1f, 0.1f);
        }

        if (dagger != null && energyBar != null)
        {
            float pct = dagger.currentEnergy / dagger.maxEnergy;
            energyBar.value = Mathf.Lerp(energyBar.value, pct, Time.deltaTime * 10f);
        }
    }

    void UpdateCombo()
    {
        if (combo > 1)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                combo = 1; kills = 0;
                comboText.gameObject.SetActive(false);
            }
        }
    }

    // ── API PUBLICA — llamar desde otros scripts ─────────

    public void AddKill()
    {
        kills++;
        comboTimer = COMBO_TIME;
        combo = kills >= 5 ? 4 : kills >= 3 ? 3 : kills >= 2 ? 2 : 1;

        int pts = 100 * combo;
        score += pts;

        if (scoreText) scoreText.text = $"PUNTOS: {score:N0}";

        if (combo > 1)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = $"x{combo} COMBO!";
        }

        // Subir nivel cada 1000 puntos
        int level = 1 + score / 1000;
        if (levelText) levelText.text = $"NIVEL {level}";

        // Texto flotante
        SpawnFloating($"+{pts}", Color.cyan);
    }

    public void OnPlayerHurt()
    {
        combo = 1; kills = 0;
        comboText.gameObject.SetActive(false);
    }

    // ── TEXTO FLOTANTE SIMPLE ────────────────────────────
    void SpawnFloating(string msg, Color col)
    {
        if (player == null) return;
        GameObject go = new GameObject("FloatText");
        go.transform.position = player.transform.position + Vector3.up * 1.5f;

        // Canvas para texto en world space
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        go.AddComponent<CanvasScaler>();

        GameObject textGO = new GameObject("T");
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = msg;
        tmp.color = col;
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;

        var rt = textGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 60);
        go.GetComponent<RectTransform>().localScale = Vector3.one * 0.02f;

        Destroy(go, 1f);
        StartCoroutine(FloatUp(go));
    }

    System.Collections.IEnumerator FloatUp(GameObject go)
    {
        float t = 0;
        Vector3 start = go.transform.position;
        while (t < 1f && go != null)
        {
            t += Time.deltaTime;
            go.transform.position = start + Vector3.up * t * 1.5f;
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp) { Color c = tmp.color; c.a = 1f - t; tmp.color = c; }
            yield return null;
        }
    }

    // ── HELPERS PARA CONSTRUIR UI ────────────────────────

    GameObject MakePanel(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go;
    }

    Slider MakeSlider(GameObject parent, string name,
        Vector2 pos, Vector2 size, Color fillColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        RectTransform bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.18f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        RectTransform faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
        faRT.offsetMin = new Vector2(0, 0); faRT.offsetMax = new Vector2(0, 0);

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fRT = fill.AddComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
        fRT.offsetMin = fRT.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = fillColor;

        Slider slider = go.AddComponent<Slider>();
        slider.fillRect = fRT;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = false;

        return slider;
    }

    TextMeshProUGUI MakeTMP(GameObject parent, string name,
        Vector2 pos, Vector2 size, string text,
        float fontSize, Color color, TextAlignmentOptions align,
        Vector2? anchorMin = null, Vector2? anchorMax = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin ?? new Vector2(1, 1);
        rt.anchorMax = anchorMax ?? new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = FontStyles.Bold;

        return tmp;
    }

    void MakeLabel(GameObject parent, string text,
        Vector2 pos, Vector2 size, Color color, float fontSize)
    {
        MakeTMP(parent, text + "Label", pos, size, text,
            fontSize, color, TextAlignmentOptions.Left,
            Vector2.zero, Vector2.zero);
    }
}
