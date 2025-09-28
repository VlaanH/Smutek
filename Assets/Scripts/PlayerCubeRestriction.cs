using UnityEngine;

public class PlayerCubeRestriction : MonoBehaviour
{
    [Header("Barrier cube (внутренний)")]
    public Transform barrierCenter;           
    public Vector3 barrierSize = Vector3.one;

    [Header("Return zone cube (внешний)")]
    public Transform zoneCenter;
    public Vector3 zoneSize = Vector3.one * 10f;

    [Header("Teleport settings")]
    public float pushSpeed = 5f;     // скорость плавного возврата внутрь
    public float border = 0.2f;      // расстояние от края барьера, где начинает работать поле

    void Update()
    {
        if (barrierCenter == null || zoneCenter == null) return;

        // --- Проверяем: игрок внутри зоны возврата? ---
        if (!IsInsideCube(transform.position, zoneCenter.position, zoneSize))
            return; // вне зоны - ничего не делаем

        // --- Если игрок вышел за барьер ---
        if (!IsInsideCube(transform.position, barrierCenter.position, barrierSize))
        {
            Vector3 clampedWorld = GetClampedPosition(transform.position, barrierCenter.position, barrierSize, border);
            transform.position = Vector3.Lerp(transform.position, clampedWorld, Time.deltaTime * pushSpeed);
        }
    }

    // Проверка: точка внутри куба?
    bool IsInsideCube(Vector3 pos, Vector3 center, Vector3 size)
    {
        Vector3 half = size * 0.5f;
        Vector3 local = pos - center;
        return (Mathf.Abs(local.x) <= half.x &&
                Mathf.Abs(local.y) <= half.y &&
                Mathf.Abs(local.z) <= half.z);
    }

    // Возвращает ближайшую допустимую точку внутри куба
    Vector3 GetClampedPosition(Vector3 pos, Vector3 center, Vector3 size, float border)
    {
        Vector3 local = pos - center;
        Vector3 half = size * 0.5f;

        float cx = Mathf.Clamp(local.x, -half.x + border, half.x - border);
        float cy = Mathf.Clamp(local.y, -half.y + border, half.y - border);
        float cz = Mathf.Clamp(local.z, -half.z + border, half.z - border);

        return center + new Vector3(cx, cy, cz);
    }

    void OnDrawGizmos()
    {
        if (barrierCenter != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(barrierCenter.position, barrierSize);
        }

        if (zoneCenter != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(zoneCenter.position, zoneSize);
        }
    }
}

