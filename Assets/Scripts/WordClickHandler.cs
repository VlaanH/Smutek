using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class WordClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("Настройки выделения")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    
    [Header("Настройки автоматического размера контейнера")]
    [SerializeField] private bool autoResizeEnabled = true;
    [SerializeField] private float topPadding = 10f;
    [SerializeField] private float bottomPadding = 10f;
    [SerializeField] private bool keepWidth = true; // Сохранять ширину, изменять только высоту
    
    // Публичный список выбранных слов
    public List<string> selectedWords = new List<string>();
    
    private TextMeshProUGUI tmpText;
    private RectTransform rectTransform;
    private HashSet<int> selectedWordIndices = new HashSet<int>(); // Индексы выбранных слов для отслеживания

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        
        // Настраиваем автоматическое изменение размера контейнера, если включено
        if (autoResizeEnabled)
        {
            SetupAutoResize();
        }
    }

    void Start()
    {
        // Обновляем размер контейнера при старте
        if (autoResizeEnabled)
        {
            ResizeToFitText();
        }
    }

    private void SetupAutoResize()
    {
        // Включаем перенос слов для корректного расчета высоты
        tmpText.enableWordWrapping = true;
        
        // Устанавливаем режим переполнения
        tmpText.overflowMode = TextOverflowModes.Overflow;
    }

    private void ResizeToFitText()
    {
        if (!autoResizeEnabled) return;
        
        // Принудительно обновляем текст для получения корректных размеров
        tmpText.ForceMeshUpdate();
        
        // Получаем предпочтительную высоту текста
        float preferredHeight = tmpText.GetPreferredValues().y;
        
        // Добавляем отступы
        float totalHeight = preferredHeight + topPadding + bottomPadding;
        
        // Получаем текущий размер
        Vector2 currentSize = rectTransform.sizeDelta;
        
        // Изменяем только высоту или всю область
        Vector2 newSize = keepWidth ? 
            new Vector2(currentSize.x, totalHeight) : 
            new Vector2(tmpText.GetPreferredValues().x + 20f, totalHeight);
        
        // Применяем новый размер
        rectTransform.sizeDelta = newSize;
        
        Debug.Log($"Размер контейнера изменен на: {newSize} (высота текста: {preferredHeight})");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Определяем позицию курсора
        Vector3 mousePos = Input.mousePosition;
        
        // Находим индекс слова
        int wordIndex = TMP_TextUtilities.FindIntersectingWord(tmpText, mousePos, eventData.pressEventCamera);
        
        if (wordIndex != -1)
        {
            ToggleWordSelection(wordIndex);
        }
        else
        {
            Debug.Log("Клик мимо слов");
        }
    }

    private void ToggleWordSelection(int wordIndex)
    {
        // Получаем информацию о слове
        TMP_WordInfo wordInfo = tmpText.textInfo.wordInfo[wordIndex];
        string word = GetCleanWord(wordInfo);
        
        if (selectedWordIndices.Contains(wordIndex))
        {
            // Убираем выделение
            RemoveWordSelection(wordIndex, word);
        }
        else
        {
            // Добавляем выделение
            AddWordSelection(wordIndex, word);
        }
        
        // Обновляем визуальное отображение
        UpdateTextDisplay();
        
        // Выводим текущий список в консоль для отладки
        Debug.Log("Текущий список слов: " + string.Join(", ", selectedWords));
    }

    private void AddWordSelection(int wordIndex, string word)
    {
        selectedWordIndices.Add(wordIndex);
        selectedWords.Add(word);
        
        Debug.Log($"Выбрано слово: {word}");
    }

    private void RemoveWordSelection(int wordIndex, string word)
    {
        selectedWordIndices.Remove(wordIndex);
        selectedWords.Remove(word);
        
        Debug.Log($"Убрано выделение слова: {word}");
    }

    private void UpdateTextDisplay()
    {
        // Сбрасываем все цвета
        for (int i = 0; i < tmpText.textInfo.wordCount; i++)
        {
            TMP_WordInfo wordInfo = tmpText.textInfo.wordInfo[i];
            
            Color32 color = selectedWordIndices.Contains(i) ? highlightColor : normalColor;
            
            // Применяем цвет ко всем символам слова
            for (int j = wordInfo.firstCharacterIndex; j <= wordInfo.lastCharacterIndex; j++)
            {
                int materialIndex = tmpText.textInfo.characterInfo[j].materialReferenceIndex;
                int vertexIndex = tmpText.textInfo.characterInfo[j].vertexIndex;
                
                Color32[] vertexColors = tmpText.textInfo.meshInfo[materialIndex].colors32;
                
                vertexColors[vertexIndex + 0] = color;
                vertexColors[vertexIndex + 1] = color;
                vertexColors[vertexIndex + 2] = color;
                vertexColors[vertexIndex + 3] = color;
            }
        }
        
        // Обновляем отображение
        tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private string GetCleanWord(TMP_WordInfo wordInfo)
    {
        // Извлекаем текст слова и очищаем от знаков препинания
        string word = "";
        for (int i = wordInfo.firstCharacterIndex; i <= wordInfo.lastCharacterIndex; i++)
        {
            word += tmpText.textInfo.characterInfo[i].character;
        }
        
        // Убираем знаки препинания и приводим к нижнему регистру
        return word.Trim().ToLower().Trim(',', '.', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']');
    }

    // Публичные методы для управления списком
    public void ClearAllSelections()
    {
        selectedWordIndices.Clear();
        selectedWords.Clear();
        
        UpdateTextDisplay();
        Debug.Log("Все выделения очищены");
    }

    public int GetSelectedWordsCount()
    {
        return selectedWords.Count;
    }

    public bool IsWordSelected(string word)
    {
        return selectedWords.Contains(word.ToLower().Trim());
    }

    // Метод для программного добавления слов
    public void SelectWordByText(string word)
    {
        string cleanWord = word.ToLower().Trim();
        
        for (int i = 0; i < tmpText.textInfo.wordCount; i++)
        {
            string currentWord = GetCleanWord(tmpText.textInfo.wordInfo[i]);
            if (currentWord == cleanWord && !selectedWordIndices.Contains(i))
            {
                AddWordSelection(i, currentWord);
                UpdateTextDisplay();
                break;
            }
        }
    }

    // Метод для получения уникальных слов (без повторений)
    public List<string> GetUniqueSelectedWords()
    {
        HashSet<string> uniqueWords = new HashSet<string>(selectedWords);
        return new List<string>(uniqueWords);
    }

    // Методы для управления автоматическим изменением размера контейнера
    public void SetText(string newText)
    {
        tmpText.text = newText;
        if (autoResizeEnabled)
        {
            // Ждем один кадр, чтобы текст успел обновиться
            StartCoroutine(ResizeAfterFrame());
        }
    }
    
    private System.Collections.IEnumerator ResizeAfterFrame()
    {
        yield return null; // Ждем один кадр
        ResizeToFitText();
    }

    public void SetAutoResizeEnabled(bool enabled)
    {
        autoResizeEnabled = enabled;
        if (enabled)
        {
            SetupAutoResize();
            ResizeToFitText();
        }
    }

    public void SetPadding(float top, float bottom)
    {
        topPadding = top;
        bottomPadding = bottom;
        
        if (autoResizeEnabled)
        {
            ResizeToFitText();
        }
    }

    public void RefreshContainerSize()
    {
        if (autoResizeEnabled)
        {
            ResizeToFitText();
        }
    }
    
    // Получить текущую высоту текста
    public float GetTextHeight()
    {
        tmpText.ForceMeshUpdate();
        return tmpText.GetPreferredValues().y;
    }

    // Вызывается при изменении размера RectTransform или текста
    void OnRectTransformDimensionsChange()
    {
        if (autoResizeEnabled && tmpText != null)
        {
            ResizeToFitText();
        }
    }
}