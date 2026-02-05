using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttractionType
{
    Shop,       // Магазин
    Entrance,   // Подъезд
    Garage,     // Гараж
    Other       // Другое
}

public class AttractionPoint : MonoBehaviour
{
    [Header("Настройки точки притяжения")]
    [SerializeField] private AttractionType attractionType = AttractionType.Other;
    [SerializeField] private float waitTimeMin = 2f;
    [SerializeField] private float waitTimeMax = 5f;
    [SerializeField] private AudioClip arrivalSound;

    [Header("Ограничение количества NPC")]
    [SerializeField] private int maxNPCsAtPoint = 3;
    [SerializeField] private bool unlimitedCapacity = false;
    
    [Header("Анимация на точке")]
    [SerializeField] private RuntimeAnimatorController animatorControllerAtPoint;
    [SerializeField] private bool useDefaultAnimatorWhenLeaving = true;
    [Tooltip("Если не указан, NPC вернется к своему оригинальному контроллеру")]
    [SerializeField] private RuntimeAnimatorController defaultAnimatorController;

    [Header("Визуализация")]
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private float gizmoRadius = 0.5f;
    [SerializeField] private bool showOccupancyInfo = true;

    // Список NPC находящихся на точке
    private List<NPCCityWalker> occupyingNPCs = new List<NPCCityWalker>();

    public AttractionType Type => attractionType;
    public float GetRandomWaitTime() => Random.Range(waitTimeMin, waitTimeMax);
    public AudioClip ArrivalSound => arrivalSound;
    public RuntimeAnimatorController AnimatorControllerAtPoint => animatorControllerAtPoint;
    public RuntimeAnimatorController DefaultAnimatorController => defaultAnimatorController;
    public bool UseDefaultAnimatorWhenLeaving => useDefaultAnimatorWhenLeaving;

    /// <summary>
    /// Проверяет, есть ли свободное место на точке
    /// </summary>
    public bool HasFreeSpace()
    {
        if (GetOccupancyPercentage()==0)
            return true;

        // Очищаем список от null объектов
        occupyingNPCs.RemoveAll(npc => npc == null);

        return occupyingNPCs.Count < maxNPCsAtPoint;
    }

    /// <summary>
    /// Получить текущее количество NPC на точке
    /// </summary>
    public int GetCurrentOccupancy()
    {
        occupyingNPCs.RemoveAll(npc => npc == null);
        return occupyingNPCs.Count;
    }

    /// <summary>
    /// Получить процент заполненности точки
    /// </summary>
    public float GetOccupancyPercentage()
    {
        if (unlimitedCapacity)
            return 0f;

        occupyingNPCs.RemoveAll(npc => npc == null);
        return (float)occupyingNPCs.Count / maxNPCsAtPoint * 100f;
    }

    /// <summary>
    /// Получить максимальную вместимость точки
    /// </summary>
    public int GetMaxCapacity()
    {
        return unlimitedCapacity ? int.MaxValue : maxNPCsAtPoint;
    }

    /// <summary>
    /// Попытка занять место на точке
    /// </summary>
    public bool TryOccupy(NPCCityWalker npc)
    {
        if (!HasFreeSpace())
        {
            return false;
        }

        if (!occupyingNPCs.Contains(npc))
        {
            occupyingNPCs.Add(npc);
            return true;
        }

        return true; // Уже занято этим NPC
    }

    /// <summary>
    /// Освободить место на точке
    /// </summary>
    public void Release(NPCCityWalker npc)
    {
        if (occupyingNPCs.Contains(npc))
        {
            occupyingNPCs.Remove(npc);
        }
    }

    /// <summary>
    /// Принудительно очистить все занятые места
    /// </summary>
    public void ClearAllOccupants()
    {
        occupyingNPCs.Clear();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        
        // Рисуем иконку типа точки
        Gizmos.DrawRay(transform.position, Vector3.up * 2f);

        #if UNITY_EDITOR
        if (showOccupancyInfo && Application.isPlaying)
        {
            // Визуализация заполненности
            if (!unlimitedCapacity)
            {
                occupyingNPCs.RemoveAll(npc => npc == null);
                
                // Цвет в зависимости от заполненности
                if (occupyingNPCs.Count >= maxNPCsAtPoint)
                {
                    Gizmos.color = Color.red; // Заполнено
                }
                else if (occupyingNPCs.Count > 0)
                {
                    Gizmos.color = Color.yellow; // Частично занято
                }
                else
                {
                    Gizmos.color = Color.green; // Свободно
                }

                // Рисуем индикатор заполненности
                Vector3 indicatorPos = transform.position + Vector3.up * 2.5f;
                Gizmos.DrawSphere(indicatorPos, 0.15f);

                // Показываем количество
                for (int i = 0; i < occupyingNPCs.Count; i++)
                {
                    if (occupyingNPCs[i] != null)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(transform.position, occupyingNPCs[i].transform.position);
                    }
                }
            }
        }
        #endif
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, gizmoRadius);

        #if UNITY_EDITOR
        if (!unlimitedCapacity)
        {
            // Показываем зону вокруг точки
            Gizmos.color = new Color(0, 1, 1, 0.2f);
            Gizmos.DrawWireSphere(transform.position, gizmoRadius * 2f);

            // Отображаем информацию в Scene view
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 3f,
                $"{gameObject.name}\nТип: {attractionType}\nМест: {GetCurrentOccupancy()}/{maxNPCsAtPoint}",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12
                }
            );
        }
        else
        {
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 3f,
                $"{gameObject.name}\nТип: {attractionType}\nМест: Без ограничений",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12
                }
            );
        }
        #endif
    }

    void OnDestroy()
    {
        // Освобождаем всех NPC при уничтожении точки
        foreach (var npc in occupyingNPCs)
        {
            if (npc != null)
            {
                npc.OnAttractionPointDestroyed(this);
            }
        }
        occupyingNPCs.Clear();
    }

    #if UNITY_EDITOR
    // Для отладки в инспекторе
    [ContextMenu("Show Occupancy Info")]
    void ShowOccupancyInfo()
    {
        occupyingNPCs.RemoveAll(npc => npc == null);
        Debug.Log($"=== {gameObject.name} ===");
        Debug.Log($"Занято: {occupyingNPCs.Count}/{maxNPCsAtPoint}");
        Debug.Log($"Свободно мест: {HasFreeSpace()}");
        
        for (int i = 0; i < occupyingNPCs.Count; i++)
        {
            Debug.Log($"  {i + 1}. {occupyingNPCs[i].gameObject.name}");
        }
    }

    [ContextMenu("Clear All Occupants")]
    void ClearOccupantsMenu()
    {
        ClearAllOccupants();
        Debug.Log($"{gameObject.name}: Все места освобождены");
    }
    #endif
}