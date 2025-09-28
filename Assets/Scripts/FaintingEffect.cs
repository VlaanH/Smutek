using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Random = UnityEngine.Random;

public class FaintingEffect : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioSource audioSource;        // сюда назначаем AudioSource
    public AudioClip faintLoopClip;        // звук для обморока
    public AudioClip recoveryClip;         // звук восстановления (опционально)
    
    [Header("Volume (PostProcessing v2)")]
    public PostProcessVolume volume; // назначьте Volume с Profile
    public bool overrideVolumeWeight = false; // если хотим изменять вес Volume (вместо/в дополнение)
    
    [Header("Player Controller)")]
    public FirstPersonController FirstPersonControllerObj;
    public Transform CameraPlayer; 
    
    [Header("Timings")]
    public float totalDuration = 80f; // общее время восстановления — длиннее, чтобы «отпускало» медленно
    public float peakHold = 5f;     // сколько держим максимум перед началом восстановления

    [Header("Recovery Shape")]
    public AnimationCurve recoveryCurve = new AnimationCurve(
        new Keyframe(0f, 1f),       // старт — полный эффект
        new Keyframe(0.3f, 0.6f),   // быстро чуть отпустило
        new Keyframe(0.7f, 0.2f),   // остаточные эффекты
        new Keyframe(1f, 0f)        // совсем прошло
    );
    [Header("Camera Wobble Settings")]
    public float descentSpeed = 25f; // Скорость опускания (градусов в секунду)
    public float maxDescentAngle = 45f; // Максимальный угол опускания

    [Header("Intensity (start values)")]
    [Range(0f, 1f)] public float startChromatic = 1f;
    [Range(0f, 1f)] public float startGrain = 1f;

    [Header("Camera disturbance")]
    public Transform camTransform;
    public float maxTiltDeg = 6f;
    public float tiltFrequency = 6f;

    // internal refs
    ChromaticAberration chroma;
    Grain filmGrain;
    float initialVolumeWeight = 1f;

    void Awake()
    {
        if (volume == null)
        {
            Debug.LogError("PostProcessVolume не назначен.");
            enabled = false;
            return;
        }

        // попытка получить overrides
        if (!volume.profile.TryGetSettings(out chroma))
        {
            Debug.LogWarning("В Profile не найден ChromaticAberration. Добавьте его.");
        }

        if (!volume.profile.TryGetSettings(out filmGrain))
        {
            Debug.LogWarning("В Profile не найден FilmGrain. Добавьте его.");
        }

        initialVolumeWeight = volume.weight;
        if (camTransform == null) camTransform = Camera.main?.transform;
    }

    // запуск эффекта извне
    public IEnumerator TriggerFaint(bool isCameraCanMovControl = true)
    {
        FirstPersonControllerObj.cameraCanMove = false;
        
        StopAllCoroutines();

        StartCoroutine(FaintRoutine());
        
        yield return new WaitForSeconds(totalDuration+peakHold);
        
        FirstPersonControllerObj.cameraCanMove = isCameraCanMovControl;
    }

    IEnumerator FaintRoutine()
    {
        // запуск звука обморока
        if (audioSource != null && faintLoopClip != null)
        {
            audioSource.clip = faintLoopClip;
            audioSource.loop = true;
            audioSource.Play();
        }

     
        float t = 0f;
        while (t < peakHold)
        {
            t += Time.deltaTime;
            ApplyCameraWobble(t);
            yield return null;
        }

        t = 0f;
        while (t < totalDuration)
        {
            float normalized = Mathf.Clamp01(t / totalDuration);
            float curveVal = recoveryCurve.Evaluate(normalized);

            if (chroma != null)
                chroma.intensity.value = Mathf.Lerp(0f, startChromatic, curveVal);

            if (filmGrain != null)
                filmGrain.intensity.value = Mathf.Lerp(0f, startGrain, curveVal);

            if (overrideVolumeWeight)
                volume.weight = Mathf.Lerp(0f, 1f, curveVal);

            ApplyCameraRecovery(normalized);

            t += Time.deltaTime;
            yield return null;
        }

        // выключаем эффекты
        if (chroma != null) chroma.intensity.value = 0f;
        if (filmGrain != null) filmGrain.intensity.value = 0f;
        if (overrideVolumeWeight) volume.weight = initialVolumeWeight;

        // стопаем звук обморока
        if (audioSource != null && faintLoopClip != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // звук восстановления (по желанию)
        if (audioSource != null && recoveryClip != null)
        {
            audioSource.PlayOneShot(recoveryClip);
        }
    }


    void ApplyCameraRecovery(float normalized)
    {
        if (camTransform == null) return;

        // Получаем значение кривой восстановления (от 1 до 0)
        float curveValue = recoveryCurve.Evaluate(normalized);
    
        // Плавное восстановление угла опускания (от maxDescentAngle к 0)
        float currentDescentAngle = Mathf.Lerp(0f, maxDescentAngle, curveValue);
    
        // Уменьшение интенсивности тряски по кривой
        float wobbleIntensity = curveValue;
    
        // Добавляем случайность только если есть интенсивность
        float randomFactor = wobbleIntensity > 0.01f ? (0.5f + 0.5f * Random.value) : 0f;
        float wob = Mathf.Sin(Time.time * tiltFrequency) * maxTiltDeg * wobbleIntensity * randomFactor;
    
        // Применяем к камере (опускание + тряска)
        camTransform.localRotation = Quaternion.Euler(currentDescentAngle + wob, 0f, 0f);
    }
    void ApplyCameraWobble(float time)
    {
        
        if (camTransform == null) return;
    
        // Эффект тряски (как было)
        float wob = Mathf.Sin(time * tiltFrequency) * maxTiltDeg * (0.5f + 0.5f * Random.value);
    
        // Постепенное опускание камеры с ограничением
        float descent = Mathf.Clamp(time * descentSpeed, 0f, maxDescentAngle);
    
        // Комбинируем тряску и опускание
        camTransform.localRotation = Quaternion.Euler(wob + descent, 0f, 0f);
    }
 

  


   
}
