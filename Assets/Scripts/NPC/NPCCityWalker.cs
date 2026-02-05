using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCCityWalker : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.5f;
    
    [Header("Анимация")]
    [SerializeField] private Animator animator;
    [SerializeField] private string walkAnimationParameter = "IsWalking";
    [SerializeField] private string speedParameter = "Speed";

    [Header("Поиск точек")]
    [SerializeField] private int maxAttemptsToFindPoint = 5;
    [SerializeField] private bool skipFullPoints = true;

    [Header("Отладка")]
    [SerializeField] private bool showDebugInfo = true;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private CityNPCSpawner spawner;
    private NPCVariant npcVariant; // Вариант NPC с ограничениями
    private AttractionPoint currentTarget;
    private bool isWaiting = false;
    private bool isOccupyingPoint = false;
    private Coroutine behaviorCoroutine;
    
    // Для управления аниматором
    private RuntimeAnimatorController originalAnimatorController;
    private bool animatorChanged = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Сохраняем оригинальный контроллер аниматора
        if (animator != null)
        {
            originalAnimatorController = animator.runtimeAnimatorController;
        }
    }

    void Start()
    {
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.autoBraking = true;
            
            // Проверяем что агент на NavMesh
            if (!agent.isOnNavMesh)
            {
                Debug.LogError($"{gameObject.name}: КРИТИЧЕСКАЯ ОШИБКА - NavMeshAgent не на NavMesh при старте!");
                
                // Пытаемся найти ближайшую точку на NavMesh
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                    Debug.Log($"{gameObject.name}: Перемещён на NavMesh: {hit.position}");
                }
                else
                {
                    Debug.LogError($"{gameObject.name}: Не могу найти NavMesh поблизости! Удаляю NPC...");
                    Destroy(gameObject);
                    return;
                }
            }
        }
        else
        {
            Debug.LogError($"{gameObject.name}: NavMeshAgent не найден!");
        }
    }

    public void Initialize(CityNPCSpawner citySpawner, Material sidewalkMaterial, LayerMask sidewalk, NPCVariant variant)
    {
        spawner = citySpawner;
        npcVariant = variant;
        
        if (showDebugInfo && variant != null)
        {
            Debug.Log($"{gameObject.name}: Инициализирован как '{variant.variantName}'. " +
                     $"Фильтр точек: {variant.GetFilterDescription()}");
        }
        
        // Запускаем поведение
        if (behaviorCoroutine != null)
            StopCoroutine(behaviorCoroutine);
            
        behaviorCoroutine = StartCoroutine(NPCBehavior());
    }

    private IEnumerator NPCBehavior()
    {
        while (true)
        {
            // Выбираем новую цель с учетом занятости
            currentTarget = FindAvailableAttractionPoint();
            
            if (currentTarget == null)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"{gameObject.name}: Нет доступных точек притяжения!");
                yield return new WaitForSeconds(5f);
                continue;
            }

            // Пытаемся занять место на точке
            if (!currentTarget.TryOccupy(this))
            {
                if (showDebugInfo)
                    Debug.LogWarning($"{gameObject.name}: Не удалось занять место на {currentTarget.name}");
                yield return new WaitForSeconds(1f);
                continue;
            }

            isOccupyingPoint = true;

            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Иду к {currentTarget.name} ({currentTarget.Type})");

            // Идем к цели
            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(currentTarget.transform.position);
                SetWalkingAnimation(true);

                // Ждем пока дойдем
                while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                {
                    // Обновляем скорость анимации
                    UpdateAnimationSpeed();
                    yield return null;
                }

                // Остановились у цели
                SetWalkingAnimation(false);

                if (showDebugInfo)
                    Debug.Log($"{gameObject.name}: Достиг {currentTarget.name}");

                // Воспроизводим звук
                if (currentTarget.ArrivalSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(currentTarget.ArrivalSound);
                }

                // Поворачиваемся к точке
                yield return StartCoroutine(LookAtTarget(currentTarget.transform));

                // МЕНЯЕМ АНИМАТОР НА АНИМАТОР ТОЧКИ
                ChangeAnimatorController(currentTarget.AnimatorControllerAtPoint);

                // Ждем на точке
                float waitTime = currentTarget.GetRandomWaitTime();
                isWaiting = true;
                
                if (showDebugInfo)
                    Debug.Log($"{gameObject.name}: Жду {waitTime:F1} секунд на {currentTarget.name}");
                    
                yield return new WaitForSeconds(waitTime);
                isWaiting = false;

                // ВОЗВРАЩАЕМ ОРИГИНАЛЬНЫЙ АНИМАТОР ПЕРЕД УХОДОМ
                RestoreOriginalAnimatorController(currentTarget);

                // Освобождаем место на точке
                currentTarget.Release(this);
                isOccupyingPoint = false;
            }
            else
            {
                Debug.LogError($"{gameObject.name}: NavMeshAgent не на NavMesh!");
                
                // Освобождаем точку в случае ошибки
                if (isOccupyingPoint && currentTarget != null)
                {
                    currentTarget.Release(this);
                    isOccupyingPoint = false;
                }
                
                yield return new WaitForSeconds(2f);
            }

            // Небольшая пауза перед следующим маршрутом
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }

    /// <summary>
    /// Находит доступную точку притяжения (с учетом занятости и ограничений варианта)
    /// </summary>
    private AttractionPoint FindAvailableAttractionPoint()
    {
        if (spawner == null)
            return null;

        int attempts = 0;
        AttractionPoint selectedPoint = null;

        while (attempts < maxAttemptsToFindPoint)
        {
            // Получаем точку с учетом ограничений варианта NPC
            if (npcVariant != null)
            {
                selectedPoint = spawner.GetRandomAttractionPointForVariant(npcVariant);
            }
            else
            {
                selectedPoint = spawner.GetRandomAttractionPoint();
            }
            
            if (selectedPoint == null)
            {
                if (showDebugInfo && attempts == 0)
                {
                    string variantInfo = npcVariant != null ? $" для варианта '{npcVariant.variantName}'" : "";
                    Debug.LogWarning($"{gameObject.name}: Нет доступных точек{variantInfo}!");
                }
                break;
            }

            // Проверяем есть ли свободное место
            if (skipFullPoints)
            {
                if (selectedPoint.HasFreeSpace())
                {
                    if (showDebugInfo && attempts > 0)
                    {
                        Debug.Log($"{gameObject.name}: Нашел свободную точку '{selectedPoint.name}' с {attempts + 1} попытки");
                    }
                    return selectedPoint;
                }
                else
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"{gameObject.name}: Точка '{selectedPoint.name}' занята ({selectedPoint.GetCurrentOccupancy()}/{selectedPoint.GetMaxCapacity()}), ищу другую...");
                    }
                }
            }
            else
            {
                // Не проверяем занятость, просто возвращаем
                return selectedPoint;
            }

            attempts++;
        }

        // Если не нашли свободную точку после всех попыток
        if (selectedPoint != null && showDebugInfo)
        {
            Debug.LogWarning($"{gameObject.name}: Не нашел свободную точку за {maxAttemptsToFindPoint} попыток. " +
                           $"Иду к '{selectedPoint.name}' (занято: {selectedPoint.GetCurrentOccupancy()})");
        }

        return selectedPoint;
    }

    /// <summary>
    /// Меняет контроллер аниматора на указанный
    /// </summary>
    private void ChangeAnimatorController(RuntimeAnimatorController newController)
    {
        if (animator == null || newController == null)
            return;

        if (animator.runtimeAnimatorController != newController)
        {
            animator.runtimeAnimatorController = newController;
            animatorChanged = true;
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Аниматор изменен на {newController.name}");
        }
    }

    /// <summary>
    /// Восстанавливает оригинальный контроллер аниматора
    /// </summary>
    private void RestoreOriginalAnimatorController(AttractionPoint point)
    {
        if (animator == null || !animatorChanged)
            return;

        RuntimeAnimatorController controllerToRestore = null;

        // Определяем какой контроллер восстановить
        if (point != null && point.UseDefaultAnimatorWhenLeaving && point.DefaultAnimatorController != null)
        {
            controllerToRestore = point.DefaultAnimatorController;
        }
        else
        {
            controllerToRestore = originalAnimatorController;
        }

        if (controllerToRestore != null && animator.runtimeAnimatorController != controllerToRestore)
        {
            animator.runtimeAnimatorController = controllerToRestore;
            animatorChanged = false;
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Аниматор восстановлен на {controllerToRestore.name}");
        }
    }

    private IEnumerator LookAtTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position);
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float rotationSpeed = 3f;

            while (Quaternion.Angle(transform.rotation, targetRotation) > 5f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                    rotationSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }

    private void UpdateAnimationSpeed()
    {
        if (animator != null && agent != null)
        {
            float speed = agent.velocity.magnitude;
            
            if (!string.IsNullOrEmpty(speedParameter))
            {
                animator.SetFloat(speedParameter, speed);
            }
        }
    }

    private void SetWalkingAnimation(bool isWalking)
    {
        if (animator != null && !string.IsNullOrEmpty(walkAnimationParameter))
        {
            animator.SetBool(walkAnimationParameter, isWalking);
        }
    }

    public void StopMoving()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        if (behaviorCoroutine != null)
        {
            StopCoroutine(behaviorCoroutine);
            behaviorCoroutine = null;
        }

        // Освобождаем точку если она была занята
        if (isOccupyingPoint && currentTarget != null)
        {
            currentTarget.Release(this);
            isOccupyingPoint = false;
        }

        SetWalkingAnimation(false);
    }

    /// <summary>
    /// Вызывается когда точка притяжения уничтожается
    /// </summary>
    public void OnAttractionPointDestroyed(AttractionPoint point)
    {
        if (currentTarget == point)
        {
            currentTarget = null;
            isOccupyingPoint = false;
            
            // Прерываем текущее поведение и ищем новую точку
            if (behaviorCoroutine != null)
            {
                StopCoroutine(behaviorCoroutine);
                behaviorCoroutine = StartCoroutine(NPCBehavior());
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo || !Application.isPlaying)
            return;

        // Рисуем путь NavMesh
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.blue;
            Vector3[] corners = agent.path.corners;
            
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }

            // Текущая цель
            if (currentTarget != null)
            {
                Gizmos.color = isWaiting ? Color.yellow : Color.green;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
                Gizmos.DrawWireSphere(currentTarget.transform.position, 0.5f);
            }
        }

        // Статус NPC
        Color statusColor = Color.green;
        if (isWaiting)
            statusColor = Color.red;
        else if (isOccupyingPoint)
            statusColor = Color.yellow;

        Gizmos.color = statusColor;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.2f);

        // Показываем если аниматор изменен
        if (animatorChanged)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2.5f, Vector3.one * 0.3f);
        }
    }

    void OnDestroy()
    {
        // Освобождаем точку при уничтожении NPC
        if (isOccupyingPoint && currentTarget != null)
        {
            currentTarget.Release(this);
            isOccupyingPoint = false;
        }

        if (spawner != null)
        {
            spawner.RemoveNPC(this);
        }
    }
}