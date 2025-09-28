using UnityEngine;
using UnityEngine.UI;

public class WakeUpBlink : MonoBehaviour
{
    [Header("Настройки моргания")]
    public float blinkDuration = 0.3f;      // Длительность одного моргания
    public float pauseBetweenBlinks = 1.5f; // Пауза между морганиями
    public int blinkCount = 3;              // Количество морганий при пробуждении
    public AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // Кривая моргания

    [Header("Компоненты")] 
    public GameObject eyeObj;
    public Image eyelidImage;               // UI Image для век (если используется)
    public SpriteRenderer eyelidRenderer;   // SpriteRenderer для век (если используется)
    
    private bool isBlinking = false;
    private int currentBlinkCount = 0;
    
    void Start()
    {
        
        // Автоматически найти компонент, если не назначен
        if (eyelidImage == null)
            eyelidImage = GetComponent<Image>();
        
        if (eyelidRenderer == null)
            eyelidRenderer = GetComponent<SpriteRenderer>();
       
        // Запустить эффект пробуждения
        //StartWakeUpSequence();
        
    }
    
    public void StartWakeUpSequence()
    {
        
        currentBlinkCount = 0;
        
        // Начать с закрытых глаз
        SetEyelidAlpha(1f);
        
        // Включаем глаза
        if (eyeObj != null)
            eyeObj.SetActive(true);
      
        // Запустить последовательность моргания
        InvokeRepeating(nameof(Blink), 0.5f, Random.Range(blinkDuration , blinkDuration*2)  + pauseBetweenBlinks);
        
        
    }

    public void SetBlackEye()
    {
        // Начать с закрытых глаз
        SetEyelidAlpha(1f);
        
        // Включаем глаза
        if (eyeObj != null)
            eyeObj.SetActive(true);
    }

    void Blink()
    {
        if (currentBlinkCount >= blinkCount)
        {
            CancelInvoke(nameof(Blink));
            
            // Финальное открытие глаз
            StartCoroutine(FinalEyeOpen());
            return;
        }
        
        if (!isBlinking)
        {
            StartCoroutine(BlinkCoroutine());
            currentBlinkCount++;
        }
    }
    
    System.Collections.IEnumerator BlinkCoroutine()
    {
        
        isBlinking = true;
        float elapsed = 0f;
        
        while (elapsed < blinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blinkDuration;
            float alpha = blinkCurve.Evaluate(t);
            
            SetEyelidAlpha(alpha);
            
            yield return null;
        }
        
        isBlinking = false;
    }
    
    System.Collections.IEnumerator FinalEyeOpen()
    {
        float elapsed = 0f;
        float openDuration = 1.0f;
        
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / openDuration;
            float alpha = Mathf.Lerp(1f, 0f, t);
            
            SetEyelidAlpha(alpha);
            
            yield return null;
        }
        
        // Полностью открыть глаза
        SetEyelidAlpha(0f);
        eyeObj.SetActive(false);
    }
    
    public void SetEyelidAlpha(float alpha)
    {
        if (eyelidImage != null)
        {
            Color color = eyelidImage.color;
            color.a = alpha;
            eyelidImage.color = color;
        }
        
        if (eyelidRenderer != null)
        {
            Color color = eyelidRenderer.color;
            color.a = alpha;
            eyelidRenderer.color = color;
        }
    }
    
    // Публичный метод для повторного запуска эффекта
    public void RestartWakeUpEffect()
    {
        StopAllCoroutines();
        CancelInvoke();
        StartWakeUpSequence();
        SetEyelidAlpha(0f);
    }
}