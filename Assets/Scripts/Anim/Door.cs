using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Door : MonoBehaviour
{
    public CreditsSystem CreditsSystem;
    public bool IsEndoFGame = false;
    public bool IsLock = false;
    private bool _isOpened = false;
    private DateTime _openDataTime;
    
    
    [Header("Auto Close")]
    [SerializeField] private bool enableAutoClose = true;
    [SerializeField] private float autoCloseDelay = 3f; // Время в секундах до автозакрытия
    private Coroutine autoCloseCoroutine;
    
    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _doorOpenSound;
    [SerializeField] private AudioClip _doorCloseSound;

    public void Start()
    {
        _openDataTime = DateTime.Now;
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }
    }
    
    private void PlayDoorSound(bool isCurrentlyOpen)
    {
        if (isCurrentlyOpen)
        {
            // Если дверь открыта, играем звук закрытия
            PlaySound(_doorOpenSound);
        }
        else
        {
            // Если дверь закрыта, играем звук открытия
            PlaySound(_doorCloseSound);
        }
    }
    
    private IEnumerator AutoCloseCoroutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        
        // Закрываем дверь только если она открыта
        if (_isOpened)
        {
            CloseDoor();
        }
    }
    
    private void CloseDoor()
    {
        if (_isOpened)
        {
            _isOpened = false;
            _animator.SetBool("isOpen", _isOpened);
            PlayDoorSound(false);
            
            // Останавливаем корутину автозакрытия, так как дверь уже закрыта
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }
        }
    }
    
    public void Open()
    {
        if (IsLock == false)
        {
            if (IsEndoFGame == false)
            {
                var openDataTimeAndOneSecond = _openDataTime.AddSeconds(1);
                if (DateTime.Now > openDataTimeAndOneSecond)
                {
                    _openDataTime = DateTime.Now;
                    
                    // Если дверь закрыта, открываем её
                    if (!_isOpened)
                    {
                        _isOpened = true;
                        _animator.SetBool("isOpen", _isOpened);
                        PlayDoorSound(_isOpened); // Играем звук открытия
                        
                        // Запускаем автозакрытие, если оно включено
                        if (enableAutoClose)
                        {
                            // Останавливаем предыдущую корутину, если она есть
                            if (autoCloseCoroutine != null)
                            {
                                StopCoroutine(autoCloseCoroutine);
                            }
                            autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine());
                        }
                    }
                    else
                    {
                        // Если дверь открыта, закрываем её вручную
                        CloseDoor();
                    }
                }
            }
            else
            {
                CreditsSystem.StartCredits();
            }
        }
    }
    

    private void OnDestroy()
    {
        // Останавливаем корутину при уничтожении объекта
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
        }
    }
}