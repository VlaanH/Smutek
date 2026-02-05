using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCVariant
{
    [Header("Основные настройки")]
    public string variantName = "NPC Variant";
    public GameObject npcPrefab;
    [Range(0, 100)]
    public int spawnWeight = 10; // Вес для случайного выбора
    
    [Header("Ограничение по типам точек")]
    [Tooltip("Если список пустой - NPC может ходить ко всем точкам")]
    public List<AttractionType> allowedAttractionTypes = new List<AttractionType>();
    
    [Header("Ограничение по конкретным точкам")]
    [Tooltip("Если список заполнен - NPC будет ходить ТОЛЬКО к этим конкретным точкам (игнорируя типы)")]
    public List<AttractionPoint> specificAttractionPoints = new List<AttractionPoint>();
    
    [Header("Режим фильтрации")]
    public NPCPointFilterMode filterMode = NPCPointFilterMode.AllowedTypes;
    
    /// <summary>
    /// Проверяет может ли этот тип NPC посещать указанную точку
    /// </summary>
    public bool CanVisitPoint(AttractionPoint point)
    {
        if (point == null)
            return false;
        
        switch (filterMode)
        {
            case NPCPointFilterMode.AllPoints:
                // Может посещать все точки
                return true;
                
            case NPCPointFilterMode.AllowedTypes:
                // Если список типов пустой - может ходить везде
                if (allowedAttractionTypes == null || allowedAttractionTypes.Count == 0)
                    return true;
                
                // Проверяем есть ли тип точки в разрешенных
                return allowedAttractionTypes.Contains(point.Type);
                
            case NPCPointFilterMode.SpecificPoints:
                // Только конкретные точки
                if (specificAttractionPoints == null || specificAttractionPoints.Count == 0)
                    return true;
                
                return specificAttractionPoints.Contains(point);
                
            case NPCPointFilterMode.Combined:
                // Комбинированный режим: конкретные точки ИЛИ разрешенные типы
                bool inSpecificList = specificAttractionPoints != null && 
                                     specificAttractionPoints.Contains(point);
                
                bool typeAllowed = allowedAttractionTypes != null && 
                                  allowedAttractionTypes.Count > 0 && 
                                  allowedAttractionTypes.Contains(point.Type);
                
                // Если есть конкретные точки и точка в списке - разрешаем
                if (specificAttractionPoints != null && specificAttractionPoints.Count > 0)
                {
                    if (inSpecificList)
                        return true;
                }
                
                // Если есть разрешенные типы и тип подходит - разрешаем
                if (allowedAttractionTypes != null && allowedAttractionTypes.Count > 0)
                {
                    if (typeAllowed)
                        return true;
                }
                
                // Если оба списка пусты - разрешаем все
                if ((specificAttractionPoints == null || specificAttractionPoints.Count == 0) &&
                    (allowedAttractionTypes == null || allowedAttractionTypes.Count == 0))
                {
                    return true;
                }
                
                return false;
                
            default:
                return true;
        }
    }
    
    /// <summary>
    /// Получает описание ограничений для отладки
    /// </summary>
    public string GetFilterDescription()
    {
        switch (filterMode)
        {
            case NPCPointFilterMode.AllPoints:
                return "Все точки";
                
            case NPCPointFilterMode.AllowedTypes:
                if (allowedAttractionTypes == null || allowedAttractionTypes.Count == 0)
                    return "Все типы точек";
                return $"Типы: {string.Join(", ", allowedAttractionTypes)}";
                
            case NPCPointFilterMode.SpecificPoints:
                if (specificAttractionPoints == null || specificAttractionPoints.Count == 0)
                    return "Конкретные точки не указаны";
                return $"Конкретные точки: {specificAttractionPoints.Count} шт.";
                
            case NPCPointFilterMode.Combined:
                string result = "Комбинированный: ";
                if (specificAttractionPoints != null && specificAttractionPoints.Count > 0)
                    result += $"{specificAttractionPoints.Count} точек";
                if (allowedAttractionTypes != null && allowedAttractionTypes.Count > 0)
                {
                    if (specificAttractionPoints != null && specificAttractionPoints.Count > 0)
                        result += " + ";
                    result += $"Типы: {string.Join(", ", allowedAttractionTypes)}";
                }
                return result;
                
            default:
                return "Неизвестный режим";
        }
    }
}

/// <summary>
/// Режим фильтрации точек притяжения для NPC
/// </summary>
public enum NPCPointFilterMode
{
    [Tooltip("NPC может посещать любые точки без ограничений")]
    AllPoints,
    
    [Tooltip("NPC может посещать только точки определенных типов (Shop, Entrance и т.д.)")]
    AllowedTypes,
    
    [Tooltip("NPC может посещать только конкретные указанные точки")]
    SpecificPoints,
    
    [Tooltip("NPC может посещать конкретные точки ИЛИ точки разрешенных типов")]
    Combined
}

public class CityNPCSpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    [SerializeField] private List<NPCVariant> npcVariants = new List<NPCVariant>();
    [SerializeField] private int npcCount = 10;
    [SerializeField] private float spawnRadius = 100f;
    [SerializeField] private bool spawnOnlyNearAttractions = true;
    
    [Header("Тротуар")]
    [SerializeField] private Material sidewalkMaterial;
    [SerializeField] private LayerMask sidewalkLayer;
    
    [Header("Точки притяжения")]
    [SerializeField] private List<AttractionPoint> attractionPoints = new List<AttractionPoint>();
    [SerializeField] private bool autoFindAttractionPoints = true;
    
    [Header("Настройки поведения")]
    [SerializeField] private float minSpawnDelay = 0.5f;
    [SerializeField] private float maxSpawnDelay = 2f;
    [SerializeField] private bool spawnOnStart = true;

    [Header("Отладка")]
    [SerializeField] private bool drawSpawnArea = true;
    [SerializeField] private Color spawnAreaColor = Color.green;
    [SerializeField] private bool showVariantStats = true;

    private List<NPCCityWalker> activeNPCs = new List<NPCCityWalker>();
    private Dictionary<string, int> variantSpawnCount = new Dictionary<string, int>();

    void Start()
    {
        // Проверка наличия вариантов NPC
        if (npcVariants == null || npcVariants.Count == 0)
        {
            Debug.LogError("CityNPCSpawner: Список NPC вариантов пуст! Добавьте хотя бы один вариант NPC.");
            return;
        }

        // Проверка что у всех вариантов есть префабы
        bool hasValidVariants = false;
        foreach (var variant in npcVariants)
        {
            if (variant.npcPrefab != null)
            {
                hasValidVariants = true;
                variantSpawnCount[variant.variantName] = 0;
            }
        }

        if (!hasValidVariants)
        {
            Debug.LogError("CityNPCSpawner: Ни у одного варианта NPC не установлен префаб!");
            return;
        }

        // Автоматически находим точки притяжения
        if (autoFindAttractionPoints)
        {
            FindAllAttractionPoints();
        }

        if (attractionPoints.Count == 0)
        {
            Debug.LogError("CityNPCSpawner: Точки притяжения не найдены! Добавьте AttractionPoint в сцену.");
            return;
        }

        if (spawnOnStart)
        {
            StartCoroutine(SpawnNPCs());
        }
    }

    private void FindAllAttractionPoints()
    {
        AttractionPoint[] points = FindObjectsOfType<AttractionPoint>();
        attractionPoints.Clear();
        attractionPoints.AddRange(points);
        
        Debug.Log($"CityNPCSpawner: Найдено {attractionPoints.Count} точек притяжения");
    }

    private IEnumerator SpawnNPCs()
    {
        for (int i = 0; i < npcCount; i++)
        {
            SpawnSingleNPC();
            
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
        }
        
        Debug.Log($"CityNPCSpawner: Создано {activeNPCs.Count} NPC");
        
        if (showVariantStats)
        {
            LogVariantStatistics();
        }
    }

    public void SpawnSingleNPC()
    {
        // Выбираем случайный вариант NPC на основе весов
        NPCVariant selectedVariant = GetRandomNPCVariant();
        
        if (selectedVariant == null || selectedVariant.npcPrefab == null)
        {
            Debug.LogError("CityNPCSpawner: Не удалось выбрать вариант NPC!");
            return;
        }

        // Находим случайную точку на NavMesh для спавна
        Vector3 spawnPosition = FindRandomNavMeshPosition();
        
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("CityNPCSpawner: Не удалось найти позицию на NavMesh для спавна!");
            return;
        }

        // Создаем NPC
        GameObject npcObject = Instantiate(selectedVariant.npcPrefab, spawnPosition, Quaternion.identity);
        npcObject.transform.parent = transform;
        npcObject.name = $"{selectedVariant.variantName}_{activeNPCs.Count + 1}";

        // Получаем или добавляем компонент NPCCityWalker
        NPCCityWalker walker = npcObject.GetComponent<NPCCityWalker>();
        if (walker == null)
        {
            walker = npcObject.AddComponent<NPCCityWalker>();
        }

        // Проверяем что NavMeshAgent на NavMesh
        UnityEngine.AI.NavMeshAgent agent = npcObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            if (!agent.isOnNavMesh)
            {
                Debug.LogError($"CityNPCSpawner: {npcObject.name} не на NavMesh после создания! Удаляю...");
                Destroy(npcObject);
                return;
            }
        }

        // Инициализируем NPC с передачей информации о варианте
        walker.Initialize(this, sidewalkMaterial, sidewalkLayer, selectedVariant);
        
        activeNPCs.Add(walker);
        
        // Обновляем статистику
        if (variantSpawnCount.ContainsKey(selectedVariant.variantName))
        {
            variantSpawnCount[selectedVariant.variantName]++;
        }
        
        Debug.Log($"CityNPCSpawner: Создан {npcObject.name} ({selectedVariant.variantName}) на позиции {spawnPosition}");
    }

    /// <summary>
    /// Выбирает случайный вариант NPC на основе весов spawn weight
    /// </summary>
    private NPCVariant GetRandomNPCVariant()
    {
        // Фильтруем только валидные варианты
        List<NPCVariant> validVariants = new List<NPCVariant>();
        int totalWeight = 0;

        foreach (var variant in npcVariants)
        {
            if (variant.npcPrefab != null && variant.spawnWeight > 0)
            {
                validVariants.Add(variant);
                totalWeight += variant.spawnWeight;
            }
        }

        if (validVariants.Count == 0)
        {
            Debug.LogError("CityNPCSpawner: Нет валидных вариантов NPC!");
            return null;
        }

        // Случайный выбор на основе весов
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var variant in validVariants)
        {
            currentWeight += variant.spawnWeight;
            if (randomValue < currentWeight)
            {
                return variant;
            }
        }

        // На всякий случай возвращаем первый валидный вариант
        return validVariants[0];
    }

    /// <summary>
    /// Выбирает случайный вариант NPC определенного типа (опционально)
    /// </summary>
    public NPCVariant GetRandomNPCVariantByName(string variantName)
    {
        foreach (var variant in npcVariants)
        {
            if (variant.variantName == variantName && variant.npcPrefab != null)
            {
                return variant;
            }
        }
        return null;
    }

    private Vector3 FindRandomNavMeshPosition()
    {
        // СПОСОБ 1: Ищем возле точек притяжения (самый надёжный)
        if (spawnOnlyNearAttractions && attractionPoints.Count > 0)
        {
            // Выбираем случайную точку притяжения
            AttractionPoint randomPoint = attractionPoints[Random.Range(0, attractionPoints.Count)];
            
            // Проверяем что сама точка на NavMesh
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint.transform.position, out UnityEngine.AI.NavMeshHit pointHit, 2f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // Ищем позицию рядом
                for (int i = 0; i < 10; i++)
                {
                    Vector3 randomOffset = Random.insideUnitSphere * 5f;
                    randomOffset.y = 0;
                    Vector3 targetPos = pointHit.position + randomOffset;
                    
                    if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out UnityEngine.AI.NavMeshHit hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        return hit.position;
                    }
                }
                
                // Если не нашли рядом - спавним прямо на точке
                return pointHit.position;
            }
        }

        // СПОСОБ 2: Случайный поиск в радиусе спавна
        for (int i = 0; i < 30; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        // СПОСОБ 3: Прямо на центре спавнера
        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit centerHit, 20f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return centerHit.position;
        }

        Debug.LogError("CityNPCSpawner: НЕ НАЙДЕН NavMesh! Проверьте что NavMesh запечён (Baked)!");
        return Vector3.zero;
    }

    public AttractionPoint GetRandomAttractionPoint()
    {
        if (attractionPoints.Count == 0)
            return null;

        return attractionPoints[Random.Range(0, attractionPoints.Count)];
    }

    public AttractionPoint GetRandomAttractionPoint(AttractionType type)
    {
        List<AttractionPoint> filtered = attractionPoints.FindAll(p => p.Type == type);
        
        if (filtered.Count == 0)
            return GetRandomAttractionPoint();

        return filtered[Random.Range(0, filtered.Count)];
    }

    /// <summary>
    /// Получает случайную точку притяжения для конкретного варианта NPC
    /// </summary>
    public AttractionPoint GetRandomAttractionPointForVariant(NPCVariant variant)
    {
        if (variant == null || attractionPoints.Count == 0)
            return null;

        // Фильтруем точки согласно ограничениям варианта
        List<AttractionPoint> availablePoints = new List<AttractionPoint>();
        
        foreach (var point in attractionPoints)
        {
            if (point != null && variant.CanVisitPoint(point))
            {
                availablePoints.Add(point);
            }
        }

        if (availablePoints.Count == 0)
        {
            Debug.LogWarning($"CityNPCSpawner: Для варианта '{variant.variantName}' нет доступных точек! " +
                           $"Фильтр: {variant.GetFilterDescription()}");
            return null;
        }

        return availablePoints[Random.Range(0, availablePoints.Count)];
    }

    /// <summary>
    /// Получает случайную точку определенного типа для конкретного варианта NPC
    /// </summary>
    public AttractionPoint GetRandomAttractionPointForVariant(NPCVariant variant, AttractionType type)
    {
        if (variant == null || attractionPoints.Count == 0)
            return null;

        // Фильтруем точки по типу И ограничениям варианта
        List<AttractionPoint> availablePoints = new List<AttractionPoint>();
        
        foreach (var point in attractionPoints)
        {
            if (point != null && point.Type == type && variant.CanVisitPoint(point))
            {
                availablePoints.Add(point);
            }
        }

        if (availablePoints.Count == 0)
        {
            // Пробуем вернуть любую доступную точку для этого варианта
            return GetRandomAttractionPointForVariant(variant);
        }

        return availablePoints[Random.Range(0, availablePoints.Count)];
    }

    public void RemoveNPC(NPCCityWalker npc)
    {
        if (activeNPCs.Contains(npc))
        {
            activeNPCs.Remove(npc);
        }
    }

    public void ClearAllNPCs()
    {
        foreach (var npc in activeNPCs)
        {
            if (npc != null)
                Destroy(npc.gameObject);
        }
        activeNPCs.Clear();
        
        // Сбрасываем статистику
        foreach (var key in new List<string>(variantSpawnCount.Keys))
        {
            variantSpawnCount[key] = 0;
        }
    }

    private void LogVariantStatistics()
    {
        Debug.Log("=== Статистика спавна NPC ===");
        foreach (var kvp in variantSpawnCount)
        {
            float percentage = (float)kvp.Value / npcCount * 100f;
            Debug.Log($"{kvp.Key}: {kvp.Value} ({percentage:F1}%)");
        }
    }

    void OnDrawGizmos()
    {
        if (!drawSpawnArea)
            return;

        Gizmos.color = spawnAreaColor;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Рисуем линии к точкам притяжения
        if (attractionPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var point in attractionPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawLine(transform.position, point.transform.position);
                }
            }
        }

        // Проверяем NavMesh в центре
        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hit.position, 0.5f);
            Gizmos.DrawLine(transform.position, hit.position);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.5f);
            
            #if UNITY_EDITOR
            UnityEditor.EditorGUIUtility.SetIconSize(Vector2.one * 16);
            #endif
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    // Дополнительные методы для управления
    [ContextMenu("Spawn NPC Now")]
    public void SpawnNPCNow()
    {
        SpawnSingleNPC();
    }

    [ContextMenu("Clear All NPCs")]
    public void ClearNPCs()
    {
        ClearAllNPCs();
    }

    [ContextMenu("Respawn All NPCs")]
    public void RespawnAllNPCs()
    {
        ClearAllNPCs();
        StartCoroutine(SpawnNPCs());
    }

    [ContextMenu("Show Variant Statistics")]
    public void ShowStatistics()
    {
        LogVariantStatistics();
    }

    // Геттеры для информации
    public int GetActiveNPCCount() => activeNPCs.Count;
    public int GetVariantCount() => npcVariants.Count;
    public List<NPCVariant> GetNPCVariants() => npcVariants;
}