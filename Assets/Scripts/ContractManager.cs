using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContractManager : MonoBehaviour
{
    [System.Serializable]
    public class TaskResult
    {
        
        public List<string> correctSelectedWords = new List<string>();
        public List<string> wrongSelectedWords = new List<string>();
        public List<string> missedWords = new List<string>();
        public int earnedPoints = 0;
        public bool isPerfect = false;
        public float timeSpent = 0f; // в игровых минутах
        public bool completedOnTime = true;

        public string GetResultText()
        {
            string result = $"На проверке\n";
            result += $"Время выполнения: {timeSpent:F0} мин";
            if (!completedOnTime)
                result += " ⏰ (Превышен лимит времени)";
            result += "\n\n";

            if (correctSelectedWords.Count > 0)
                result += $"✓ Правильно выбрано ({correctSelectedWords.Count}): {string.Join(", ", correctSelectedWords)}\n\n";

            if (wrongSelectedWords.Count > 0)
                result += $"✗ Неправильно выбрано ({wrongSelectedWords.Count}): {string.Join(", ", wrongSelectedWords)}\n\n";

            if (missedWords.Count > 0)
                result += $"○ Пропущено ({missedWords.Count}): {string.Join(", ", missedWords)}\n\n";

            if (isPerfect)
                result += "🎉 Отлично! Все слова найдены правильно!";

            return result;
        }
    }

    [System.Serializable]
    public class ContractTask
    {
        [Header("Основная информация")]
        [TextArea(5, 10)]
        public string contractText;

        [TextArea(2, 5)]
        public string question;

        public string authorName;
        public string authorStats;

        [Header("Настройки времени")]
        [Tooltip("Время в игровых минутах для выполнения задания")]
        public float taskTimeInGameMinutes = 10f;

        [Tooltip("Штраф за превышение времени (в процентах от заработанных баллов)")]
        [Range(0f, 100f)]
        public float overtimePenaltyPercent = 25f;

        [Header("Правильные слова для выделения")]
        public List<string> correctWords = new List<string>();

        [Header("Настройки баллов")]
        public int pointsPerCorrectWord = 10;
        public int penaltyPerWrongWord = 5;
        public int bonusForPerfectAnswer = 25;

        [NonSerialized] private HashSet<string> _processedWords = null;

        public HashSet<string> GetProcessedCorrectWords()
        {
            if (_processedWords == null)
                _processedWords = ProcessCorrectWords();
            return _processedWords;
        }

        public void RefreshProcessedWords() => _processedWords = null;

        private HashSet<string> ProcessCorrectWords()
        {
            HashSet<string> processedWords = new HashSet<string>();
            foreach (string item in correctWords)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;

                string[] words = item.Trim().Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    string cleanWord = word.Trim(new char[] { '.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']', '{', '}', '-', '—', '–' }).ToLower();
                    if (!string.IsNullOrEmpty(cleanWord))
                        processedWords.Add(cleanWord);
                }
            }
            return processedWords;
        }
    }
    public bool IsStart = false;
    public bool Iscompleted = false;
    
    
    [Header("UI Элементы")]
    public TextMeshProUGUI contractTextUI;
    public Text questionTextUI;
    public Text authorTextUI;
    public Text authorStatsTextUI;
    public Text totalScoreUI;
    public Text resultTextUI;
    public Button checkButton;

    [Header("Компоненты")]
    public WordClickHandler wordClickHandler;
    public DayCycle dayCycle;

    [Header("Настройки отображения результата")]
    public float resultShowDuration = 4f;
    public GameObject resultPanel;
    public GameObject workPanel;

    [Header("Настройки времени")]
    [Tooltip("Множитель игрового времени к реальному")]
    public float gameTimeMultiplier = 1f;

    [Header("Список договоров и заданий")]
    public List<ContractTask> contractTasks = new List<ContractTask>();

    [Header("UI времени")]
    public Text taskTimerUI;
    public Text gameClockUI;
    public Text endCash;
    
    [Header("Отладка")]
    public bool showDebugInfo = false;

    private int currentTaskIndex = 0;
    private int totalScore = 0;
    private bool isProcessing = false;

    // Реальное время выполнения задания
    private float realTimePassed = 0f;
    private ContractTask currentTask = null;
    private bool isTaskActive = false;

    void Start() => InitializeSystem();

    void InitializeSystem()
    {
        IsStart = true;
        if (contractTasks.Count == 0) Debug.LogWarning("Список договоров пуст!");

        if (wordClickHandler == null) Debug.LogError("WordClickHandler не назначен!");
        if (dayCycle == null) Debug.LogError("DayCycle не назначен!");

        if (checkButton != null)
        {
            checkButton.onClick.RemoveAllListeners();
            checkButton.onClick.AddListener(CheckCurrentTask);
        }

        ShowCurrentTask();
        UpdateScoreDisplay();
        
    }

    void Update()
    {
        if (isTaskActive && currentTask != null)
        {
            realTimePassed += Time.deltaTime;

            // Переводим реальное время в игровые минуты
            float elapsedGameMinutes = realTimePassed * gameTimeMultiplier;

            float remainingGameMinutes = Mathf.Max(0f, currentTask.taskTimeInGameMinutes - elapsedGameMinutes);

            // UI таймер
            if (taskTimerUI != null)
            {
                int minutes = Mathf.FloorToInt(remainingGameMinutes);
                int seconds = Mathf.FloorToInt((remainingGameMinutes - minutes) * 60f);
                taskTimerUI.text = $"Осталось: {minutes:D2}:{seconds:D2}";
            }

            // Разблокируем кнопку проверки только после истечения времени
            if (checkButton != null)
            {
                checkButton.interactable = remainingGameMinutes <= 0f;
            }

            gameClockUI.text = dayCycle.TimeOfDay24.ToShortTimeString();
        }
    }


    void ShowCurrentTask()
    {
        if (currentTaskIndex >= contractTasks.Count)
        {
            ShowCompletionMessage();
            return;
        }

        currentTask = contractTasks[currentTaskIndex];
        realTimePassed = 0f;
        isTaskActive = true;

        if (contractTextUI != null) wordClickHandler.SetText(currentTask.contractText);
        if (questionTextUI != null) questionTextUI.text = currentTask.question;
        if (authorTextUI != null) authorTextUI.text = currentTask.authorName;
        if (authorStatsTextUI != null) authorStatsTextUI.text = currentTask.authorStats;

        wordClickHandler.ClearAllSelections();
        if (resultTextUI != null) resultTextUI.text = "";
        if (checkButton != null) checkButton.interactable = false;

        if (showDebugInfo)
        {
            HashSet<string> words = currentTask.GetProcessedCorrectWords();
            Debug.Log($"Показано задание {currentTaskIndex + 1}/{contractTasks.Count}: {currentTask.authorName}");
            Debug.Log($"Время на выполнение: {currentTask.taskTimeInGameMinutes} минут");
        }
    }

    public void CheckCurrentTask()
    {
        if (!isTaskActive || isProcessing) return;

        isProcessing = true;
        isTaskActive = false;

        float gameTimeEarned = currentTask.taskTimeInGameMinutes; // можно адаптировать, если нужно меньше времени

        dayCycle.SetTime(dayCycle.GetTimeSpan() + gameTimeEarned); // обновляем игровое время

        List<string> selectedWords = wordClickHandler.GetUniqueSelectedWords();
        bool completedOnTime = realTimePassed * gameTimeMultiplier <= currentTask.taskTimeInGameMinutes * 60f;

        TaskResult result = EvaluateTask(currentTask, selectedWords, gameTimeEarned, completedOnTime);
        totalScore += result.earnedPoints;

        StartCoroutine(ShowResultAndProceed(result));
    }

    TaskResult EvaluateTask(ContractTask task, List<string> selectedWords, float timeSpent, bool completedOnTime)
    {
        TaskResult result = new TaskResult
        {
            timeSpent = timeSpent,
            completedOnTime = completedOnTime
        };

        HashSet<string> correctWordsSet = task.GetProcessedCorrectWords();
        HashSet<string> selectedSet = new HashSet<string>();
        foreach (string word in selectedWords)
        {
            string cleanWord = word.Trim(new char[] { '.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']', '{', '}', '-', '—', '–' }).ToLower();
            if (!string.IsNullOrEmpty(cleanWord)) selectedSet.Add(cleanWord);
        }

        foreach (string w in selectedSet) if (correctWordsSet.Contains(w)) result.correctSelectedWords.Add(w); else result.wrongSelectedWords.Add(w);
        foreach (string w in correctWordsSet) if (!selectedSet.Contains(w)) result.missedWords.Add(w);

        int points = result.correctSelectedWords.Count * task.pointsPerCorrectWord;
        int penalty = result.wrongSelectedWords.Count * task.penaltyPerWrongWord;
        result.earnedPoints = Mathf.Max(0, points - penalty);

        if (result.wrongSelectedWords.Count == 0 && result.missedWords.Count == 0 && result.correctSelectedWords.Count > 0)
        {
            result.earnedPoints += task.bonusForPerfectAnswer;
            result.isPerfect = true;
        }

        if (!completedOnTime && result.earnedPoints > 0)
        {
            int timePenalty = Mathf.RoundToInt(result.earnedPoints * task.overtimePenaltyPercent / 100f);
            result.earnedPoints = Mathf.Max(0, result.earnedPoints - timePenalty);
        }

        return result;
    }

    IEnumerator ShowResultAndProceed(TaskResult result)
    {
        if (checkButton != null) checkButton.interactable = false;
        if (resultTextUI != null) resultTextUI.text = result.GetResultText();
       

        UpdateScoreDisplay();
        yield return new WaitForSeconds(resultShowDuration);

        

        currentTaskIndex++;
        ShowCurrentTask();
        isProcessing = false;
    }

    void UpdateScoreDisplay()
    {
        if (totalScoreUI != null) totalScoreUI.text = $"Выполнено заданий: {currentTaskIndex}";
    }

    void ShowCompletionMessage()
    {
        Debug.Log($"Все задания выполнены! Итоговый счет: {totalScore}");
        if (contractTextUI != null) contractTextUI.text = "Все договоры проверены!";
        if (questionTextUI != null) questionTextUI.text = $"Поздравляем! Вы набрали {totalScore} баллов.";
        if (authorTextUI != null) authorTextUI.text = "Система оценки";
        if (checkButton != null) checkButton.gameObject.SetActive(false);
        if (resultTextUI != null) resultTextUI.text = $"🏆 Итоговый счет: {totalScore}";

        if (workPanel != null) workPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(true);
        endCash.text ="За обучение: "+totalScore.ToString() + "Р"; 
        
        Iscompleted = true;
    }
}
