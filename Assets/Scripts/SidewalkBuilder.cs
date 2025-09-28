using UnityEngine;
using System.Collections.Generic;

public class SidewalkBuilder : MonoBehaviour
{
    [Header("Настройки тротуара")]
    [SerializeField] private GameObject tilePrefab; // Префаб плитки тротуара
    [SerializeField] private int tilesCount = 10; // Количество плиток
    [SerializeField] private float gapBetweenTiles = 0f; // Зазор между плитками (0 = плотно)
    [SerializeField] private bool useCustomSpacing = false; // Использовать кастомное расстояние
    [SerializeField] private Vector3 customSpacing = new Vector3(2f, 0f, 0f); // Кастомное расстояние (если включено)
    
    [Header("Направление построения")]
    [SerializeField] private BuildDirection direction = BuildDirection.Forward;
    
    [Header("Настройки плоскости")]
    [SerializeField] private BuildPlane buildPlane = BuildPlane.XZ; // Плоскость построения
    [SerializeField] private float planeOffset = 0f; // Смещение по третьей оси
    
    [Header("Дополнительные настройки")]
    [SerializeField] private bool randomRotation = false; // Случайный поворот плиток
    [SerializeField] private Vector3 rotationRange = new Vector3(0, 5, 0); // Диапазон случайного поворота
    [SerializeField] private bool parentToThis = true; // Делать ли этот объект родителем для плиток
    
    private List<GameObject> spawnedTiles = new List<GameObject>();
    private Vector3 prefabSize; // Размер префаба
    
    public enum BuildDirection
    {
        Forward,
        Backward,
        Left,
        Right,
        Up,
        Down
    }
    
    public enum BuildPlane
    {
        XZ, // Горизонтальная плоскость (земля)
        XY, // Вертикальная плоскость (стена спереди)
        YZ  // Боковая плоскость (стена сбоку)
    }
    
    void Start()
    {
        if (tilePrefab != null)
        {
            CalculatePrefabSize();
            BuildSidewalk();
        }
        else
        {
            Debug.LogError("Префаб плитки не назначен!");
        }
    }
    
    private void CalculatePrefabSize()
    {
        if (tilePrefab == null) return;
        
        // Получаем размер через Renderer или Collider
        Renderer renderer = tilePrefab.GetComponent<Renderer>();
        if (renderer != null)
        {
            prefabSize = renderer.bounds.size;
        }
        else
        {
            // Если нет Renderer, пробуем Collider
            Collider collider = tilePrefab.GetComponent<Collider>();
            if (collider != null)
            {
                prefabSize = collider.bounds.size;
            }
            else
            {
                // Если ничего нет, используем размер по умолчанию
                prefabSize = Vector3.one;
                Debug.LogWarning("У префаба нет Renderer или Collider. Используется размер по умолчанию (1,1,1)");
            }
        }
        
        Debug.Log($"Размер префаба: {prefabSize}");
    }
    
    [ContextMenu("Построить тротуар")]
    public void BuildSidewalk()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Префаб плитки не назначен!");
            return;
        }
        
        CalculatePrefabSize();
        ClearExistingTiles();
        
        Vector3 currentPosition = transform.position;
        Vector3 buildDirectionVector = GetDirectionVector();
        
        for (int i = 1; i < tilesCount; i++)
        {
            // Вычисляем позицию для текущей плитки с учетом размера
            Vector3 tilePosition = CalculateTilePosition(currentPosition, buildDirectionVector, i);
            
            // Создаем плитку
            GameObject tile = Instantiate(tilePrefab, tilePosition, tilePrefab.transform.rotation);
          
    
            
            
            // Настраиваем родительский объект
            if (parentToThis)
            {
                tile.transform.SetParent(transform);
            }
            
            // Добавляем в список
            spawnedTiles.Add(tile);
            
            // Применяем случайный поворот если включен
            if (randomRotation)
            {
                ApplyRandomRotation(tile);
            }
            
            // Именуем плитку для удобства
            tile.name = $"Tile_{i:00}";
        }
        
        Debug.Log($"Построен тротуар из {tilesCount} плиток");
    }
    
    private Vector3 CalculateTilePosition(Vector3 startPos, Vector3 dirVector, int index)
    {
        // Для первого элемента (index = 0) возвращаем стартовую позицию
        if (index == 0)
        {
            return GetPositionWithPlaneOffset(startPos);
        }
        
        // Для остальных элементов вычисляем расстояние на основе размера префаба
        float distance;
        
        if (useCustomSpacing)
        {
            // Используем кастомное расстояние
            distance = GetDistanceForDirection(customSpacing, dirVector);
        }
        else
        {
            // Используем размер префаба + зазор
            distance = GetDistanceForDirection(GetSpacingFromPrefabSize(), dirVector);
        }
        
        // Каждый следующий объект смещается на полный размер предыдущего
        Vector3 totalOffset = dirVector * (distance * index);
        
        return GetPositionWithPlaneOffset(startPos + totalOffset);
    }
    
    private Vector3 GetPositionWithPlaneOffset(Vector3 basePos)
    {
        // Применяем настройки плоскости
        switch (buildPlane)
        {
            case BuildPlane.XZ:
                return new Vector3(basePos.x, basePos.y + planeOffset, basePos.z);
            case BuildPlane.XY:
                return new Vector3(basePos.x, basePos.y, basePos.z + planeOffset);
            case BuildPlane.YZ:
                return new Vector3(basePos.x + planeOffset, basePos.y, basePos.z);
            default:
                return basePos;
        }
    }
    
    private float GetDistanceForDirection(Vector3 spacing, Vector3 direction)
    {
        // Возвращаем размер в нужном направлении
        Vector3 absDirection = new Vector3(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));
        
        return spacing.x * absDirection.x + 
               spacing.y * absDirection.y + 
               spacing.z * absDirection.z;
    }
    
    private Vector3 GetSpacingFromPrefabSize()
    {
        // Добавляем зазор к размеру префаба
        return new Vector3(
            prefabSize.x + gapBetweenTiles,
            prefabSize.y + gapBetweenTiles,
            prefabSize.z + gapBetweenTiles
        );
    }
    
    private Vector3 GetDirectionVector()
    {
        switch (buildPlane)
        {
            case BuildPlane.XZ: // Горизонтальная плоскость
                switch (direction)
                {
                    case BuildDirection.Forward: return Vector3.forward;
                    case BuildDirection.Backward: return Vector3.back;
                    case BuildDirection.Left: return Vector3.left;
                    case BuildDirection.Right: return Vector3.right;
                    default: return Vector3.forward;
                }
            
            case BuildPlane.XY: // Вертикальная плоскость (фронтальная)
                switch (direction)
                {
                    case BuildDirection.Up: return Vector3.up;
                    case BuildDirection.Down: return Vector3.down;
                    case BuildDirection.Left: return Vector3.left;
                    case BuildDirection.Right: return Vector3.right;
                    default: return Vector3.right;
                }
            
            case BuildPlane.YZ: // Боковая плоскость
                switch (direction)
                {
                    case BuildDirection.Up: return Vector3.up;
                    case BuildDirection.Down: return Vector3.down;
                    case BuildDirection.Forward: return Vector3.forward;
                    case BuildDirection.Backward: return Vector3.back;
                    default: return Vector3.forward;
                }
                
            default:
                return Vector3.forward;
        }
    }
    
    private Quaternion GetTileRotation()
    {
        // Базовая ротация в зависимости от плоскости
        switch (buildPlane)
        {
            case BuildPlane.XZ:
                return Quaternion.identity;
            case BuildPlane.XY:
                return Quaternion.Euler(90, 0, 0);
            case BuildPlane.YZ:
                return Quaternion.Euler(0, 0, 90);
            default:
                return Quaternion.identity;
        }
    }
    
    private void ApplyRandomRotation(GameObject tile)
    {
        Vector3 randomRot = new Vector3(
            Random.Range(-rotationRange.x, rotationRange.x),
            Random.Range(-rotationRange.y, rotationRange.y),
            Random.Range(-rotationRange.z, rotationRange.z)
        );
        
        tile.transform.Rotate(randomRot);
    }
    
    [ContextMenu("Очистить тротуар")]
    public void ClearExistingTiles()
    {
        // Удаляем существующие плитки
        foreach (GameObject tile in spawnedTiles)
        {
            if (tile != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(tile);
                }
                else
                {
                    DestroyImmediate(tile);
                }
            }
        }
        spawnedTiles.Clear();
    }
    
    [ContextMenu("Пересчитать размер префаба")]
    public void RecalculatePrefabSize()
    {
        CalculatePrefabSize();
    }
    
    // Вспомогательные методы для изменения параметров во время выполнения
    public void SetTileCount(int count)
    {
        tilesCount = Mathf.Max(1, count);
    }
    
    public void SetGapBetweenTiles(float gap)
    {
        gapBetweenTiles = gap;
    }
    
    public void SetBuildDirection(BuildDirection newDirection)
    {
        direction = newDirection;
    }
    
    public void SetBuildPlane(BuildPlane newPlane)
    {
        buildPlane = newPlane;
    }
  /*
    void OnDrawGizmosSelected()
    {
        // Визуализация направления построения в редакторе (только при выборе объекта)
        if (tilePrefab == null || tilesCount > 100) return; // Ограничение на количество
        
        if (prefabSize == Vector3.zero)
        {
            CalculatePrefabSize();
        }
        
        Gizmos.color = Color.yellow;
        Vector3 start = transform.position;
        Vector3 dirVector = GetDirectionVector();
        
        // Рисуем кубики показывающие размещение плиток
        int maxTiles = Mathf.Min(tilesCount, 50); // Максимум 50 для производительности
        for (int i = 0; i < maxTiles; i++)
        {
            Vector3 pos = CalculateTilePosition(start, dirVector, i);
            
            // Рисуем куб размером с префаб
            Gizmos.DrawWireCube(pos, prefabSize);
            
            if (i > 0)
            {
                Vector3 prevPos = CalculateTilePosition(start, dirVector, i - 1);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(prevPos, pos);
                Gizmos.color = Color.yellow;
            }
        }
    }*/
}