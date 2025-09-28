using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Serialization;

public class BedController : MonoBehaviour
{
    [Header("Bed Settings")]
    public Transform layingPosition;        // Точка где персонаж будет лежать
  
    
    [Header("Lying Animation Settings")]
    public float layDownDuration = 1.5f;   // Время перехода в положение лежа
    public float getUpDuration = 1.0f;     // Время вставания
    public AnimationCurve layDownCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Camera Settings")]
    public Vector3 lyingCameraOffset = new Vector3(0, -0.5f, 0); // Смещение камеры в положении лежа
    public float lyingCameraAngle = 15f;    // Угол наклона камеры при лежании
    
    [Header("UI Settings")]
    public GameObject interactionPrompt;    // UI подсказка для взаимодействия
    public string promptText = "Нажмите E чтобы лечь";
    public string getUpText = "Нажмите E чтобы встать";
    
    private FirstPersonController fpsController;
    [FormerlySerializedAs("isLaying")] private bool IsLaying = false;
    private bool isTransitioning = false;

    
    // Сохраненные значения для восстановления
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private float originalCrouchHeight;
    private bool originalPlayerCanMove;
    private bool originalEnableJump;
    private bool originalEnableCrouch;
    private bool originalEnableSprint;
    
    void Start()
    {
        // Находим контроллер первого лица
        fpsController = FindObjectOfType<FirstPersonController>();
        
        if (fpsController == null)
        {
            Debug.LogError("FirstPersonController не найден! Убедитесь, что он есть в сцене.");
            return;
        }
        
        // Сохраняем оригинальную высоту приседания
        originalCrouchHeight = fpsController.crouchHeight;
        
        // Скрываем подсказку в начале
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (fpsController == null || isTransitioning) return;
        if (Input.GetKeyDown(Settings.SelectedSettings.InteractionKeyCode))
            if (IsLaying)
            {
                StartCoroutine(GetUpFromBed());
            }
    }

    public void HandleInput()
    {
        if (fpsController == null || isTransitioning) return;
        if (Input.GetKeyDown(Settings.SelectedSettings.InteractionKeyCode))
        {
            if (!IsLaying)
            {
                StartCoroutine(LayDownOnBed());
            }
            
        }
    }
    
    public IEnumerator LayDownOnBed()
    {
        if (layingPosition == null)
        {
            Debug.LogWarning("Laying position не установлена!");
            yield break;
        }
        
        isTransitioning = true;
        
        // Сохраняем текущие настройки контроллера
        SaveControllerState();
        
        // Отключаем движение игрока
        DisablePlayerMovement();
        
        // Плавно перемещаем игрока к кровати
        yield return StartCoroutine(MovePlayerToBed());
        
        // Применяем эффект лежания
        ApplyLyingState();
        
        IsLaying = true;
        isTransitioning = false;
        
        Debug.Log("Персонаж лег на кровать");
    }
    
    IEnumerator GetUpFromBed()
    {
        isTransitioning = true;
        
        // Убираем эффект лежания
        RemoveLyingState();
        
        // Плавно поднимаем игрока
        yield return StartCoroutine(MovePlayerUp());
        
        // Восстанавливаем настройки контроллера
        RestoreControllerState();
        
        IsLaying = false;
        isTransitioning = false;
            
        Debug.Log("Персонаж встал с кровати");
    }
    
    IEnumerator MovePlayerToBed()
    {
        Transform playerTransform = fpsController.transform;
        Camera playerCamera = fpsController.playerCamera;
        
        Vector3 startPos = playerTransform.position;
        Quaternion startRot = playerTransform.rotation;
        Vector3 startCamPos = playerCamera.transform.localPosition;
        Quaternion startCamRot = playerCamera.transform.localRotation;
        
        Vector3 targetPos = layingPosition.position;
        Quaternion targetRot = layingPosition.rotation;
        Vector3 targetCamPos = startCamPos + lyingCameraOffset;
        Quaternion targetCamRot = Quaternion.Euler(lyingCameraAngle, 0, 0);
        
        float elapsed = 0f;
        
        while (elapsed < layDownDuration)
        {
            elapsed += Time.deltaTime;
            float progress = layDownCurve.Evaluate(elapsed / layDownDuration);
            
            // Перемещаем игрока
            playerTransform.position = Vector3.Lerp(startPos, targetPos, progress);
            playerTransform.rotation = Quaternion.Lerp(startRot, targetRot, progress);
            
            // Перемещаем камеру
            playerCamera.transform.localPosition = Vector3.Lerp(startCamPos, targetCamPos, progress);
            playerCamera.transform.localRotation = Quaternion.Lerp(startCamRot, targetCamRot, progress);
            
            yield return null;
        }
        
        // Устанавливаем финальные позиции
        playerTransform.position = targetPos;
        playerTransform.rotation = targetRot;
        playerCamera.transform.localPosition = targetCamPos;
        playerCamera.transform.localRotation = targetCamRot;
    }
    
    IEnumerator MovePlayerUp()
    {
        Transform playerTransform = fpsController.transform;
        Camera playerCamera = fpsController.playerCamera;
        
        Vector3 startPos = playerTransform.position;
        Quaternion startRot = playerTransform.rotation;
        Vector3 startCamPos = playerCamera.transform.localPosition;
        Quaternion startCamRot = playerCamera.transform.localRotation;
        
        Vector3 targetPos = originalPosition;
        Quaternion targetRot = originalRotation;
        Vector3 targetCamPos = originalCameraPosition;
        Quaternion targetCamRot = originalCameraRotation;
        
        float elapsed = 0f;
        
        while (elapsed < getUpDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / getUpDuration;
            
            // Перемещаем игрока обратно
            playerTransform.position = Vector3.Lerp(startPos, targetPos, progress);
            playerTransform.rotation = Quaternion.Lerp(startRot, targetRot, progress);
            
            // Восстанавливаем камеру
            playerCamera.transform.localPosition = Vector3.Lerp(startCamPos, targetCamPos, progress);
            playerCamera.transform.localRotation = Quaternion.Lerp(startCamRot, targetCamRot, progress);
            
            yield return null;
        }
        
        // Устанавливаем финальные позиции
        playerTransform.position = targetPos;
        playerTransform.rotation = targetRot;
        playerCamera.transform.localPosition = targetCamPos;
        playerCamera.transform.localRotation = targetCamRot;
    }
    
    void SaveControllerState()
    {
        // Сохраняем позиции
        originalPosition = fpsController.transform.position;
        originalRotation = fpsController.transform.rotation;
        originalCameraPosition = fpsController.playerCamera.transform.localPosition;
        originalCameraRotation = fpsController.playerCamera.transform.localRotation;
        
        // Сохраняем настройки контроллера
        originalPlayerCanMove = fpsController.playerCanMove;
        originalEnableJump = fpsController.enableJump;
        originalEnableCrouch = fpsController.enableCrouch;
        originalEnableSprint = fpsController.enableSprint;
    }
    
    void DisablePlayerMovement()
    {
        fpsController.FreezingPlayer(true);
    
        // Дополнительно останавливаем физику
        Rigidbody rb = fpsController.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    
    void ApplyLyingState()
    {
        // Изменяем высоту "приседания" для имитации лежания
        fpsController.crouchHeight = 0.3f;
        
        // Принудительно активируем "приседание"
        if (!fpsController.GetType().GetField("isCrouched", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(fpsController).Equals(true))
        {
            fpsController.Crouch();
        }
        
        // Отключаем движение камеры
        fpsController.cameraCanMove = false;
    }
    
    void RemoveLyingState()
    {
        // Восстанавливаем высоту приседания
        fpsController.crouchHeight = originalCrouchHeight;
        
        // Если был активирован crouch, отключаем его
        var isCrouchedField = fpsController.GetType().GetField("isCrouched", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (isCrouchedField.GetValue(fpsController).Equals(true))
        {
            fpsController.Crouch();
        }
        
        // Включаем движение камеры
        fpsController.cameraCanMove = true;
    }
    
    void RestoreControllerState()
    {
        // Восстанавливаем настройки контроллера
        fpsController.FreezingPlayer(false);
    }
    
    void ShowInteractionPrompt(string text)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            
            var promptTextComponent = interactionPrompt.GetComponentInChildren<UnityEngine.UI.Text>();
            if (promptTextComponent != null)
                promptTextComponent.text = text;
            
            var promptTMPComponent = interactionPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (promptTMPComponent != null)
                promptTMPComponent.text = text;
        }
    }
    
    void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
    
    // Публичные методы для внешнего вызова
    public void ForceLieDown()
    {
        if (!IsLaying && !isTransitioning)
            StartCoroutine(LayDownOnBed());
    }
    
    public void ForceGetUp()
    {
        if (IsLaying && !isTransitioning)
            StartCoroutine(GetUpFromBed());
    }
    
    public bool IsPlayerLaying()
    {
        return IsLaying;
    }
    
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    // Визуализация зоны взаимодействия в редакторе
    void OnDrawGizmosSelected()
    {

        // Позиция лежания
        if (layingPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(layingPosition.position, Vector3.one * 0.5f);
            Gizmos.DrawRay(layingPosition.position, layingPosition.forward * 1f);
            
            // Линия от кровати к позиции лежания
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, layingPosition.position);
        }
    }
}