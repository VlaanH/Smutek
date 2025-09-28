using System.Collections;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Object Type")]
    public ObjectType objectType = ObjectType.Drink;

    [Header("Interaction Settings")]
    public float rotationSpeed = 100f;
    public Vector3 holdRotation = Vector3.zero; // базовый поворот при удержании

    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip useSound;
    public AudioClip cancelSound;

    [Header("References")]
    public Camera playerCamera;
    public Transform holdPoint; // пустышка перед камерой (scale = 1,1,1)
    


    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private Vector3 originalScale;

    private bool isBeingHeld = false;
    public bool isUsed = false;
    private AudioSource audioSource;
    private Collider objectCollider;
    private Rigidbody objectRigidbody;
    private FirstPersonController playerController;


    // для пользовательского поворота
    private Quaternion additionalRotation = Quaternion.identity;

    // Статическая переменная для отслеживания активного объекта
    private static InteractableObject currentlyActiveObject = null;

    public enum ObjectType
    {
        Drink,
        Food
    }

    void Start()
    {
        // Сохраняем изначальные параметры объекта
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
        originalScale = transform.localScale;

        // Получаем компоненты
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        objectCollider = GetComponent<Collider>();
        objectRigidbody = GetComponent<Rigidbody>();

        // Находим контроллер игрока
        playerController = FindObjectOfType<FirstPersonController>();

        // Находим камеру если не указана
        if (playerCamera == null)
            playerCamera = Camera.main;

        // Если holdPoint не задан — создаём
        if (holdPoint == null && playerCamera != null)
        {
            GameObject hp = new GameObject("HoldPoint");
            hp.transform.SetParent(playerCamera.transform);
            hp.transform.localPosition = new Vector3(0, 0, 1.5f);
            hp.transform.localRotation = Quaternion.identity;
            holdPoint = hp.transform;
        }
    }

    void Update()
    {
        if (isBeingHeld && !isUsed && currentlyActiveObject == this)
        {
            HandleRotation();
            UpdateObjectPosition();

            // ESC или E — отмена
            if (Input.GetKeyDown(KeyCode.E))
            {
                OnUseObject(false);
              
            }
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(0)) // ЛКМ для вращения
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            // накапливаем дельту поворота
            Quaternion delta = Quaternion.Euler(-mouseY, mouseX, 0);
            additionalRotation = delta * additionalRotation;
        }
    }

    void UpdateObjectPosition()
    {
        // ставим объект в позицию holdPoint
        transform.position = holdPoint.position;
        transform.rotation = holdPoint.rotation * Quaternion.Euler(holdRotation) * additionalRotation;
    }

    public void StartInteraction()
    {
        if (isUsed || currentlyActiveObject != null)
            return;

        currentlyActiveObject = this;

        // Отключаем управление игроком
        if (playerController != null)
        {
            playerController.FreezingPlayer(true);
            playerController.cameraCanMove = false;
            playerController.lockCursor = false;
            
            MenuPaused.menuBlock = true;
        }

        isBeingHeld = true;

        // Отключаем физику объекта
        if (objectCollider != null) objectCollider.enabled = false;
        if (objectRigidbody != null) objectRigidbody.isKinematic = true;

        PlaySound(pickupSound);

        // Показываем UI
        InteractionUIController.Instance.ShowUI(this);

        // Включаем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // сбрасываем поворот при новом взаимодействии
        additionalRotation = Quaternion.identity;
    }

    public void OnUseObject(bool useObject)
    {
        if (currentlyActiveObject != this) return;

        if (objectType == ObjectType.Drink)
        {
            PlaySound(useObject ? useSound : cancelSound);
            EndInteraction();
        }
        else if (objectType == ObjectType.Food)
        {
            if (useObject)
            {
                PlaySound(useSound);
                StartCoroutine(ConsumeObject());
            }
            else
            {
                PlaySound(cancelSound);
                EndInteraction();
            }
        }

        isUsed = true;
        
       
    }

    IEnumerator ConsumeObject()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            yield return null;
        }

        EndInteraction();
        Destroy(gameObject);
    }

    void EndInteraction()
    {
        isBeingHeld = false;

        if (currentlyActiveObject == this)
            currentlyActiveObject = null;

        if (playerController != null)
        {
            playerController.FreezingPlayer(false);
            playerController.cameraCanMove = true;
            playerController.lockCursor = true;
            
            MenuPaused.menuBlock = false;
        }
       
        InteractionUIController.Instance.HideUI();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if ((objectType == ObjectType.Food && !isUsed) || objectType == ObjectType.Drink)
        {
            ReturnToOriginalPosition();
        }

        // сбрасываем накопленный поворот
        additionalRotation = Quaternion.identity;
    }

    void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        if (objectCollider != null) objectCollider.enabled = true;
        if (objectRigidbody != null) objectRigidbody.isKinematic = false;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void OnInteract()
    {
        if (!isUsed && currentlyActiveObject == null)
        {
           
            StartInteraction();
        }
    }

    public static void ResetActiveObject()
    {
        currentlyActiveObject = null;
    }

    public bool IsActive()
    {
        return currentlyActiveObject == this;
    }

    void OnDestroy()
    {
        if (currentlyActiveObject == this)
            currentlyActiveObject = null;
    }
}
