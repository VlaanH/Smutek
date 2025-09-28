using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CreditEntry
{
    [Header("Текст")]
    public string title = "Заголовок";
    [TextArea(3, 5)]
    public string description = "Описание или список имен";
    
    [Header("Настройки отображения")]
    public float displayDuration = 3f;
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 0.5f;
    
    [Header("Стиль текста")]
    public Color titleColor = Color.white;
    public Color descriptionColor = Color.gray;
    public int titleFontSize = 36;
    public int descriptionFontSize = 24;
}

public class CreditsSystem : MonoBehaviour
{
    [Header("Игрок")]
    [SerializeField] private FirstPersonController firstPersonController;
    
    [Header("UI Элементы")]
    [SerializeField] private Canvas creditsCanvas;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button skipButton;
    [SerializeField] private Slider progressSlider;
    
    [Header("Фоновые настройки")]
    [SerializeField] private Sprite[] backgroundSprites;
    [SerializeField] private Color backgroundColor = Color.black;
    [SerializeField] private bool useGradientBackground = true;
    [SerializeField] private Color gradientTopColor = Color.black;
    [SerializeField] private Color gradientBottomColor = new Color(0.1f, 0.1f, 0.2f, 1f);
    
    [Header("Анимация фона")]
    [SerializeField] private bool animateBackground = true;
    [SerializeField] private float backgroundScrollSpeed = 10f;
    [SerializeField] private Vector2 scrollDirection = Vector2.up;
    
    [Header("Музыка и звуки")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip creditsMusic;
    [SerializeField] private AudioClip skipSound;
    
    [Header("Титры")]
    [SerializeField] private List<CreditEntry> creditEntries = new List<CreditEntry>();
    
    [Header("Общие настройки")]
    [SerializeField] private float delayBetweenEntries = 1f;
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private bool autoReturn = true;
    [SerializeField] private string returnSceneName = "MainMenu";
    
    private Coroutine creditsCoroutine;
    private bool isPlaying = false;
    private int currentEntryIndex = 0;
    private RawImage backgroundRawImage;
    private Material backgroundMaterial;
    
    private void Awake()
    {
        SetupUI();
        SetupBackground();
    }
    
    private void Start()
    {
        if (creditsCanvas != null)
            creditsCanvas.gameObject.SetActive(false);

        
    }
    
    private void SetupUI()
    {
  
        // Настраиваем кнопку пропуска
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipCredits);
            skipButton.gameObject.SetActive(allowSkip);
        }
        
        // Настраиваем слайдер прогресса
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
            progressSlider.maxValue = 1f;
        }
    }
    
    private void SetupBackground()
    {
        if (backgroundImage == null) return;
        
        if (useGradientBackground)
        {
            // Создаем градиентный материал
            backgroundMaterial = new Material(Shader.Find("UI/Default"));
            backgroundImage.material = backgroundMaterial;
            
            // Создаем градиентную текстуру
            Texture2D gradientTexture = CreateGradientTexture();
            backgroundImage.sprite = Sprite.Create(gradientTexture, 
                new Rect(0, 0, gradientTexture.width, gradientTexture.height), 
                Vector2.one * 0.5f);
        }
        else
        {
            backgroundImage.color = backgroundColor;
        }
        
        // Добавляем RawImage для анимированного фона если нужно
        if (animateBackground && backgroundSprites.Length > 0)
        {
            GameObject rawImageGO = new GameObject("AnimatedBackground");
            rawImageGO.transform.SetParent(backgroundImage.transform, false);
            backgroundRawImage = rawImageGO.AddComponent<RawImage>();
            RectTransform rawRect = rawImageGO.GetComponent<RectTransform>();
            rawRect.anchorMin = Vector2.zero;
            rawRect.anchorMax = Vector2.one;
            rawRect.offsetMin = Vector2.zero;
            rawRect.offsetMax = Vector2.zero;
            
            if (backgroundSprites.Length > 0)
                backgroundRawImage.texture = backgroundSprites[0].texture;
        }
    }
    
    private Texture2D CreateGradientTexture()
    {
        Texture2D texture = new Texture2D(1, 256);
        for (int i = 0; i < texture.height; i++)
        {
            float t = (float)i / (texture.height - 1);
            Color color = Color.Lerp(gradientBottomColor, gradientTopColor, t);
            texture.SetPixel(0, i, color);
        }
        texture.Apply();
        return texture;
    }
    
    private void Update()
    {
        if (isPlaying && Input.GetKeyDown(skipKey) && allowSkip)
        {
            SkipCredits();
        }
        
        // Анимация фона
        if (animateBackground && backgroundRawImage != null)
        {
            Rect uvRect = backgroundRawImage.uvRect;
            uvRect.position += scrollDirection.normalized * backgroundScrollSpeed * Time.deltaTime;
            backgroundRawImage.uvRect = uvRect;
        }
    }
    
    public void StartCredits()
    {
        firstPersonController.crosshair = false;
        if (isPlaying) return;
        
        creditsCanvas.gameObject.SetActive(true);
        isPlaying = true;
        currentEntryIndex = 0;
        
        // Запускаем музыку
        if (musicSource != null && creditsMusic != null)
        {
            musicSource.clip = creditsMusic;
            musicSource.Play();
        }
        
        creditsCoroutine = StartCoroutine(PlayCredits());
    }
    
    public void SkipCredits()
    {
        if (!isPlaying) return;
        
        // Воспроизводим звук пропуска
        if (musicSource != null && skipSound != null)
        {
            musicSource.PlayOneShot(skipSound);
        }
        
        StopCredits();
    }
    
    public void StopCredits()
    {
        if (creditsCoroutine != null)
        {
            StopCoroutine(creditsCoroutine);
            creditsCoroutine = null;
        }
        
        isPlaying = false;
        creditsCanvas.gameObject.SetActive(false);
        
        // Останавливаем музыку
        if (musicSource != null)
        {
            musicSource.Stop();
        }
        
        // Возвращаемся в главное меню если нужно
        if (autoReturn && !string.IsNullOrEmpty(returnSceneName))
        {
            MainMenuConroller.LoadMainMenuScene();
        }
    }
    
    private IEnumerator PlayCredits()
    {
        float totalDuration = CalculateTotalDuration();
        
        for (int i = 0; i < creditEntries.Count; i++)
        {
            currentEntryIndex = i;
            yield return StartCoroutine(ShowCreditEntry(creditEntries[i]));
            
            // Обновляем прогресс
            if (progressSlider != null)
            {
                float progress = (float)(i + 1) / creditEntries.Count;
                progressSlider.value = progress;
            }
            
            if (i < creditEntries.Count - 1)
                yield return new WaitForSeconds(delayBetweenEntries);
        }
        
        // Завершаем титры
        yield return new WaitForSeconds(2f);
        StopCredits();
    }
    
    private IEnumerator ShowCreditEntry(CreditEntry entry)
    {
        // Настраиваем текст
        titleText.text = entry.title;
        titleText.color = new Color(entry.titleColor.r, entry.titleColor.g, entry.titleColor.b, 255);
        titleText.fontSize = entry.titleFontSize;
        
        descriptionText.text = entry.description;
        descriptionText.color = new Color(entry.descriptionColor.r, entry.descriptionColor.g, entry.descriptionColor.b, 255);
        descriptionText.fontSize = entry.descriptionFontSize;
        
        // Появление
        yield return StartCoroutine(FadeInTexts(entry));
        
        // Отображение
        yield return new WaitForSeconds(entry.displayDuration);
        
        // Исчезновение
        yield return StartCoroutine(FadeOutTexts(entry));
    }
    
    private IEnumerator FadeInTexts(CreditEntry entry)
    {
        float elapsed = 0f;
        
        while (elapsed < entry.fadeInTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / entry.fadeInTime);
            
            Color titleColor = entry.titleColor;
            titleColor.a = alpha;
            titleText.color = titleColor;
            
            Color descColor = entry.descriptionColor;
            descColor.a = alpha;
            descriptionText.color = descColor;
            
            yield return null;
        }
    }
    
    private IEnumerator FadeOutTexts(CreditEntry entry)
    {
        float elapsed = 0f;
        
        while (elapsed < entry.fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / entry.fadeOutTime));
            
            Color titleColor = entry.titleColor;
            titleColor.a = alpha;
            titleText.color = titleColor;
            
            Color descColor = entry.descriptionColor;
            descColor.a = alpha;
            descriptionText.color = descColor;
            
            yield return null;
        }
    }
    
    private float CalculateTotalDuration()
    {
        float total = 0f;
        for (int i = 0; i < creditEntries.Count; i++)
        {
            total += creditEntries[i].fadeInTime + creditEntries[i].displayDuration + creditEntries[i].fadeOutTime;
            if (i < creditEntries.Count - 1)
                total += delayBetweenEntries;
        }
        return total;
    }
    
  
}