using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaypointData
{
    public Transform waypoint;             // Ссылка на трансформ точки
    public bool shouldWait = true;         // Нужно ли ждать на этой точке
    public float waitTime = 1f;            // Время ожидания именно для этой точки
    public AudioClip soundOnReach;         // Звук при достижении точки
}

public class NPCPathMovement : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool loopPath = true;       // Зациклить маршрут
    [SerializeField] private bool reverseOnEnd = false;  // Развернуть в конце пути

    [Header("Путь следования (через объекты)")]
    [SerializeField] private List<WaypointData> waypoints = new List<WaypointData>();

    [Header("Отладка")]
    [SerializeField] private bool drawPath = true;
    [SerializeField] private Color pathColor = Color.green;

    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private bool movingForward = true;
    private Coroutine moveCoroutine;
    private AudioSource audioSource;

    void Start()
    {
        if (waypoints.Count > 0)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            StartMoving();
        }
        else
        {
            Debug.LogWarning("NPC Path Movement: Путь не задан!");
        }
    }

    public void StartMoving()
    {
        if (!isMoving && waypoints.Count > 0)
        {
            moveCoroutine = StartCoroutine(MoveAlongPath());
        }
    }

    public void StopMoving()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        isMoving = false;
    }

    private IEnumerator MoveAlongPath()
    {
        isMoving = true;

        while (isMoving)
        {
            WaypointData waypointData = waypoints[currentWaypointIndex];
            Vector3 targetPosition = waypointData.waypoint.position;

            // Поворот
            yield return StartCoroutine(RotateToTarget(targetPosition));

            // Движение
            yield return StartCoroutine(MoveToTarget(targetPosition));

            // Воспроизведение звука
            if (waypointData.soundOnReach != null && audioSource != null)
            {
                audioSource.PlayOneShot(waypointData.soundOnReach);
            }

            // Ожидание на точке (если включено)
            if (waypointData.shouldWait && waypointData.waitTime > 0)
            {
                yield return new WaitForSeconds(waypointData.waitTime);
            }

            // Переход к следующей точке
            if (!GetNextWaypoint())
            {
                break;
            }
        }

        isMoving = false;
    }

    private IEnumerator RotateToTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    rotationSpeed * Time.deltaTime);
                yield return null;
            }

            transform.rotation = targetRotation;
        }
    }

    private IEnumerator MoveToTarget(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition,
                moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
    }

    private bool GetNextWaypoint()
    {
        if (movingForward)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Count)
            {
                if (loopPath)
                {
                    currentWaypointIndex = 0;
                }
                else if (reverseOnEnd)
                {
                    movingForward = false;
                    currentWaypointIndex = waypoints.Count - 2;
                }
                else
                {
                    return false;
                }
            }
        }
        else
        {
            currentWaypointIndex--;

            if (currentWaypointIndex < 0)
            {
                if (loopPath)
                {
                    movingForward = true;
                    currentWaypointIndex = 1;
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    void OnDrawGizmos()
    {
        if (!drawPath || waypoints == null || waypoints.Count < 2)
            return;

        Gizmos.color = pathColor;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i].waypoint != null && waypoints[i + 1].waypoint != null)
            {
                Gizmos.DrawLine(waypoints[i].waypoint.position, waypoints[i + 1].waypoint.position);
                Gizmos.DrawWireSphere(waypoints[i].waypoint.position, 0.3f);
            }
        }

        if (waypoints[waypoints.Count - 1].waypoint != null)
            Gizmos.DrawWireSphere(waypoints[waypoints.Count - 1].waypoint.position, 0.3f);

        if (loopPath && waypoints.Count > 2 &&
            waypoints[0].waypoint != null && waypoints[waypoints.Count - 1].waypoint != null)
        {
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].waypoint.position, waypoints[0].waypoint.position);
        }

        if (Application.isPlaying && currentWaypointIndex < waypoints.Count &&
            waypoints[currentWaypointIndex].waypoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(waypoints[currentWaypointIndex].waypoint.position, 0.5f);
        }
    }
}
