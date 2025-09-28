using UnityEngine;
using System.Collections;

public class BackgroundMusic : MonoBehaviour
{
    [Header("Настройки музыки")]
    [SerializeField] private AudioClip[] musicTracks; // Массив музыкальных треков
    [SerializeField] private bool playOnStart = true; // Запускать музыку при старте
    [SerializeField] private bool loop = true; // Зациклить музыку
    [SerializeField] private bool shuffleMode = false; // Случайное воспроизведение
    
    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float fadeSpeed = 1f; // Скорость затухания/появления
    
    [Header("Переходы между треками")]
    [SerializeField] private float crossfadeDuration = 2f; // Длительность crossfade
    
    private AudioSource primaryAudioSource;
    private AudioSource secondaryAudioSource;
    private int currentTrackIndex = 0;
    private bool isPlaying = false;
    private bool isCrossfading = false;
    
    // Singleton pattern для глобального доступа
    public static BackgroundMusic Instance { get; private set; }
    
    void Awake()
    {
        // Проверяем, есть ли уже экземпляр BackgroundMusic
        if (Instance == null)
        {
            
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject); // Удаляем дубликат
        }
    }
    
    
    void InitializeAudioSources()
    {
        // Создаем два AudioSource для плавных переходов
        primaryAudioSource = gameObject.AddComponent<AudioSource>();
        secondaryAudioSource = gameObject.AddComponent<AudioSource>();
        
        // Настраиваем AudioSource
        SetupAudioSource(primaryAudioSource);
        SetupAudioSource(secondaryAudioSource);
    }
    
    void SetupAudioSource(AudioSource audioSource)
    {
        audioSource.playOnAwake = false;
        audioSource.loop = false; // Управляем зацикливанием вручную
        audioSource.volume = volume;
    }
    
    void Update()
    {
        // Проверяем, закончился ли текущий трек
        if (isPlaying && !isCrossfading && !primaryAudioSource.isPlaying && !secondaryAudioSource.isPlaying)
        {
            if (loop)
            {
                PlayNextTrack();
            }
            else
            {
                StopMusic();
            }
        }
    }
    
    // Публичные методы управления музыкой
    
    public void PlayMusic()
    {
        if (musicTracks.Length == 0) return;
        
        if (!isPlaying)
        {
            isPlaying = true;
            PlayTrack(currentTrackIndex);
        }
    }
    
    public void StopMusic()
    {
        isPlaying = false;
        StartCoroutine(FadeOut(primaryAudioSource));
        StartCoroutine(FadeOut(secondaryAudioSource));
    }
    
    public void PauseMusic()
    {
        primaryAudioSource.Pause();
        secondaryAudioSource.Pause();
    }
    
    public void ResumeMusic()
    {
        primaryAudioSource.UnPause();
        secondaryAudioSource.UnPause();
    }
    
    public void PlayNextTrack()
    {
        if (musicTracks.Length <= 1) return;
        
        int nextIndex;
        if (shuffleMode)
        {
            do {
                nextIndex = Random.Range(0, musicTracks.Length);
            } while (nextIndex == currentTrackIndex);
        }
        else
        {
            nextIndex = (currentTrackIndex + 1) % musicTracks.Length;
        }
        
        StartCoroutine(CrossfadeToTrack(nextIndex));
    }
    
    public void PlayPreviousTrack()
    {
        if (musicTracks.Length <= 1) return;
        
        int prevIndex = (currentTrackIndex - 1 + musicTracks.Length) % musicTracks.Length;
        StartCoroutine(CrossfadeToTrack(prevIndex));
    }
    
    public void PlaySpecificTrack(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= musicTracks.Length) return;
        
        StartCoroutine(CrossfadeToTrack(trackIndex));
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        primaryAudioSource.volume = volume;
        secondaryAudioSource.volume = volume;
    }
    
    public void SetShuffleMode(bool shuffle)
    {
        shuffleMode = shuffle;
    }
    
    public void SetLoopMode(bool loopMode)
    {
        loop = loopMode;
    }
    
    // Приватные методы
    
    void PlayTrack(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= musicTracks.Length) return;
        
        currentTrackIndex = trackIndex;
        primaryAudioSource.clip = musicTracks[trackIndex];
        primaryAudioSource.volume = volume;
        primaryAudioSource.Play();
    }
    
    IEnumerator CrossfadeToTrack(int trackIndex)
    {
        if (isCrossfading || trackIndex < 0 || trackIndex >= musicTracks.Length) yield break;
        
        isCrossfading = true;
        
        // Определяем, какой AudioSource сейчас играет
        AudioSource currentSource = primaryAudioSource.isPlaying ? primaryAudioSource : secondaryAudioSource;
        AudioSource newSource = currentSource == primaryAudioSource ? secondaryAudioSource : primaryAudioSource;
        
        // Запускаем новый трек
        newSource.clip = musicTracks[trackIndex];
        newSource.volume = 0f;
        newSource.Play();
        
        // Crossfade
        float timer = 0f;
        float startVolume = currentSource.volume;
        
        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / crossfadeDuration;
            
            currentSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            newSource.volume = Mathf.Lerp(0f, volume, progress);
            
            yield return null;
        }
        
        // Завершаем crossfade
        currentSource.Stop();
        currentSource.volume = volume;
        newSource.volume = volume;
        
        currentTrackIndex = trackIndex;
        isCrossfading = false;
    }
    
    IEnumerator FadeOut(AudioSource audioSource)
    {
        float startVolume = audioSource.volume;
        
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        audioSource.Stop();
        audioSource.volume = startVolume;
    }
    
    IEnumerator FadeIn(AudioSource audioSource)
    {
        audioSource.volume = 0f;
        audioSource.Play();
        
        while (audioSource.volume < volume)
        {
            audioSource.volume += volume * Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        audioSource.volume = volume;
    }
    
    // Геттеры для получения информации
    
    public bool IsPlaying() => isPlaying;
    public string GetCurrentTrackName() => musicTracks[currentTrackIndex]?.name ?? "Unknown";
    public int GetCurrentTrackIndex() => currentTrackIndex;
    public int GetTotalTracks() => musicTracks.Length;
    public float GetCurrentVolume() => volume;
    public bool IsShuffleModeOn() => shuffleMode;
    public bool IsLoopModeOn() => loop;
}