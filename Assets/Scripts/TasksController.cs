using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TasksController : MonoBehaviour
{
    [System.Serializable]
    public class TaskObj
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    [Header("UI References")]
    public GameObject Panel;
    public List<GameObject> TaskBoxes;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float slideInDuration = 0.3f;
    public float completeAnimationDuration = 0.8f;
    public float hideAnimationDuration = 0.4f;
    
    [Header("Animation Curves")]
    public AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve bounceInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Dictionary<int, Coroutine> activeAnimations = new Dictionary<int, Coroutine>();
    private bool isTaskBoxesActiv = true;
    
    private void Start()
    {
        // Инициализируем все task boxes как невидимые
        InitializeTaskBoxes();
    }

    
    private void Update()
    {
        if (Input.GetKeyDown(Settings.SelectedSettings.TaskBoxHid))
        {
            
            IsTaskBoxesHide(!isTaskBoxesActiv);
            
        }
        
    }

    public void IsTaskBoxesHide(bool isActive)
    {
        Panel.SetActive(isActive);
        isTaskBoxesActiv = isActive;
    }
    

    private void InitializeTaskBoxes()
    {
        foreach (var taskBox in TaskBoxes)
        {
            var canvasGroup = GetOrAddCanvasGroup(taskBox);
            canvasGroup.alpha = 0f;
            taskBox.transform.localScale = Vector3.zero;
            taskBox.SetActive(false);
        }
    }

    public void TaskBoxesActive(bool isActive)
    {
        
        if (isActive)
        {
            
            StartCoroutine(ShowAllTaskBoxesAnimated());
        }
        else
        {
            StartCoroutine(HideAllTaskBoxesAnimated());
        }
    }

    private IEnumerator ShowAllTaskBoxesAnimated()
    {
        Panel.SetActive(true);
        
        for (int i = 0; i < TaskBoxes.Count; i++)
        {
            if (TaskBoxes[i].activeInHierarchy)
            {
                StartCoroutine(ShowTaskBoxAnimated(i));
                yield return new WaitForSeconds(0.1f); // Небольшая задержка между появлениями
            }
        }
    }

    private IEnumerator HideAllTaskBoxesAnimated()
    {
        for (int i = TaskBoxes.Count - 1; i >= 0; i--)
        {
            if (TaskBoxes[i].activeInHierarchy)
            {
                StartCoroutine(HideTaskBoxAnimated(i, false));
                yield return new WaitForSeconds(0.05f); // Быстрое скрытие
            }
        }
        
        yield return new WaitForSeconds(hideAnimationDuration);
        Panel.SetActive(false);
    }

    public void SetTask(TaskObj taskObj, int idBox)
    {
        if (idBox < 0 || idBox >= TaskBoxes.Count) return;

        var taskBox = TaskBoxes[idBox];
        
        // Устанавливаем текст
        var elements = taskBox.GetComponentsInChildren<Text>(true);
        if (elements.Length >= 2)
        {
            elements[0].text = taskObj.Title;
            elements[1].text = taskObj.Description;
        }

        // Показываем панель если она скрыта
        if (!Panel.activeInHierarchy)
        {
            Panel.SetActive(true);
        }

        // Запускаем анимацию появления задачи
        StartCoroutine(ShowTaskBoxAnimated(idBox));
    }

    private IEnumerator ShowTaskBoxAnimated(int idBox)
    {
        if (activeAnimations.ContainsKey(idBox))
        {
            StopCoroutine(activeAnimations[idBox]);
        }

        var taskBox = TaskBoxes[idBox];
        var canvasGroup = GetOrAddCanvasGroup(taskBox);
        
        taskBox.SetActive(true);
        
        // Начальные значения
        canvasGroup.alpha = 0f;
        taskBox.transform.localScale = Vector3.zero;
        taskBox.transform.localPosition += Vector3.up * 50f; // Начинаем выше

        activeAnimations[idBox] = StartCoroutine(AnimateTaskAppearance(taskBox, canvasGroup, idBox));
        
        yield return null;
    }

    private IEnumerator AnimateTaskAppearance(GameObject taskBox, CanvasGroup canvasGroup, int idBox)
    {
        Vector3 originalPosition = taskBox.transform.localPosition - Vector3.up * 50f;
        Vector3 startPosition = taskBox.transform.localPosition;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < slideInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideInDuration;
            
            // Применяем кривую анимации
            float curveValue = bounceInCurve.Evaluate(progress);
            
            // Анимация масштаба с эффектом отскока
            float scaleValue = Mathf.Lerp(0f, 1f, curveValue);
            taskBox.transform.localScale = Vector3.one * scaleValue;
            
            // Анимация позиции
            taskBox.transform.localPosition = Vector3.Lerp(startPosition, originalPosition, progress);
            
            // Анимация прозрачности
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            
            yield return null;
        }
        
        // Финальные значения
        taskBox.transform.localScale = Vector3.one;
        taskBox.transform.localPosition = originalPosition;
        canvasGroup.alpha = 1f;
        
        // Добавляем небольшое подрагивание в конце
        yield return StartCoroutine(BounceEffect(taskBox));
        
        activeAnimations.Remove(idBox);
    }

    private IEnumerator BounceEffect(GameObject taskBox)
    {
        Vector3 originalScale = taskBox.transform.localScale;
        float bounceScale = 1.1f;
        float bounceTime = 0.15f;
        
        // Увеличение
        yield return ScaleOverTime(taskBox, originalScale, originalScale * bounceScale, bounceTime);
        
        // Возврат к нормальному размеру
        yield return ScaleOverTime(taskBox, originalScale * bounceScale, originalScale, bounceTime);
    }

    public IEnumerator CompleteTask(int idBox)
    {
        yield return CompleteTaskAnimated(idBox);
    }

    private IEnumerator CompleteTaskAnimated(int idBox)
    {
        var taskBox = TaskBoxes[idBox];
        var canvasGroup = GetOrAddCanvasGroup(taskBox);
        
        // Эффект завершения - изменение цвета и масштаба
        var images = taskBox.GetComponentsInChildren<Image>();
        
        var defaultColor = images[0].color;
        var anchoredPosition = taskBox.GetComponent<RectTransform>();
        var defaultPosition = anchoredPosition.anchoredPosition;
       
        
        Color originalColor = images.Length > 0 ? images[0].color : Color.white;
        Color completeColor = Color.grey;
        
        float elapsedTime = 0f;
        Vector3 originalScale = taskBox.transform.localScale;
        
        // Анимация завершения
        while (elapsedTime < completeAnimationDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (completeAnimationDuration * 0.5f);
            
            // Пульсация и изменение цвета
            float scale = Mathf.Lerp(1f, 1.2f, Mathf.Sin(progress * Mathf.PI));
            taskBox.transform.localScale = originalScale * scale;
            
            if (images.Length > 0)
            {
                images[0].color = Color.Lerp(originalColor, completeColor, progress);
            }
            
            yield return null;
        }
        
        // Возврат к исходному состоянию перед скрытием
        yield return StartCoroutine(ScaleOverTime(taskBox, taskBox.transform.localScale, originalScale, 0.2f));
        
        // Скрытие задачи
        yield return StartCoroutine(HideTaskBoxAnimated(idBox, true));

        images[0].color = defaultColor;
        anchoredPosition.anchoredPosition = defaultPosition;
        // taskBox.transform. = defaultPosition.localPosition;
    }

    public void HideTask(int idBox)
    {
        if (idBox < 0 || idBox >= TaskBoxes.Count) return;
        
        StartCoroutine(HideTaskBoxAnimated(idBox, false));
    }

    private IEnumerator HideTaskBoxAnimated(int idBox, bool isCompleted)
    {
        if (activeAnimations.ContainsKey(idBox))
        {
            StopCoroutine(activeAnimations[idBox]);
        }

        var taskBox = TaskBoxes[idBox];
        var canvasGroup = GetOrAddCanvasGroup(taskBox);
        
        Vector3 originalPosition = taskBox.transform.localPosition;
        Vector3 targetPosition = originalPosition + (isCompleted ? Vector3.down * 50f : Vector3.up * 50f);
        
        float elapsedTime = 0f;
        Vector3 originalScale = taskBox.transform.localScale;
        
        while (elapsedTime < hideAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / hideAnimationDuration;
            
            // Анимация масштаба
            float scaleValue = Mathf.Lerp(1f, 0f, progress);
            taskBox.transform.localScale = originalScale * scaleValue;
            
            // Анимация позиции
            taskBox.transform.localPosition = Vector3.Lerp(originalPosition, targetPosition, progress);
            
            // Анимация прозрачности
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            
            yield return null;
        }
        
        taskBox.SetActive(false);
        activeAnimations.Remove(idBox);
    }

    private IEnumerator ScaleOverTime(GameObject target, Vector3 fromScale, Vector3 toScale, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            target.transform.localScale = Vector3.Lerp(fromScale, toScale, progress);
            
            yield return null;
        }
        
        target.transform.localScale = toScale;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        var canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }

    // Дополнительные публичные методы для управления анимациями
    public void ShowTaskWithDelay(TaskObj taskObj, int idBox, float delay)
    {
        StartCoroutine(ShowTaskWithDelayCoroutine(taskObj, idBox, delay));
    }

    private IEnumerator ShowTaskWithDelayCoroutine(TaskObj taskObj, int idBox, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetTask(taskObj, idBox);
    }

    public void CompleteAllVisibleTasks()
    {
        StartCoroutine(CompleteAllVisibleTasksCoroutine());
    }

    private IEnumerator CompleteAllVisibleTasksCoroutine()
    {
        for (int i = 0; i < TaskBoxes.Count; i++)
        {
            if (TaskBoxes[i].activeInHierarchy)
            {
                CompleteTask(i);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}