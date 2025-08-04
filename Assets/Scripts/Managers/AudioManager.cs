using System;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class AudioManager : BaseManager
{
    [Header("Configuration")]
    public AudioSettings audioSettings;
    public AudioMixer audioMixer;
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource ambientSource;
    
    [Header("Sound Library")]
    public SoundData[] soundLibrary;
    
    private Dictionary<string, SoundData> sounds = new Dictionary<string, SoundData>();
    private Queue<AudioSource> sfxSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeSfxSources = new List<AudioSource>();
    
    private Coroutine musicFadeCoroutine;
    private string currentMusicTrack = "";
    
    //Test
    public void Start()
    {
        PlayMusic("menu_music", true);
    }
    
    protected override void OnInitialize()
    {
        Debug.Log("AudioManager initialized");
        
        // Build sound dictionary
        foreach (var soundData in soundLibrary)
        {
            sounds[soundData.name] = soundData;
        }
        
        // Create SFX source pool for better performance
        CreateSFXPool();
        
        //Connect music audio source to mixer
        musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("BGM")[0];
        
        // Load saved audio settings
        LoadAudioSettings();
        
        // Listen for scene changes to handle music transitions
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        Debug.Log($"AudioManager initialized with {sounds.Count} sounds and {audioSettings.sfxPoolSize} SFX sources");
    }

    protected override void OnReset()
    {
        // Stop all audio but keep settings
        StopAllAudio();
        LoadAudioSettings();
    }
    
    protected override void OnCleanup()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopAllAudio();
    }
    
    private void CreateSFXPool()
    {
        for (int i = 0; i < audioSettings.sfxPoolSize; i++)
        {
            GameObject sfxObject = new GameObject($"SFX_Source_{i}");
            sfxObject.transform.SetParent(transform);
            
            AudioSource source = sfxObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
            
            sfxSourcePool.Enqueue(source);
        }
    }
    
    #region Music Management
    
    public void PlayMusic(string musicName, bool fadeIn = true)
    {
        print("Playing music");
        if (!sounds.ContainsKey(musicName))
        {
            Debug.LogWarning($"Music track '{musicName}' not found in sound library");
            return;
        }
        
        if (currentMusicTrack == musicName && musicSource.isPlaying) return;
        
        SoundData musicData = sounds[musicName];
        AudioClip clipToPlay = musicData.GetRandomClip();
        
        if (clipToPlay == null)
        {
            Debug.LogWarning($"No audio clip found for music '{musicName}'");
            return;
        }
        
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }
        
        currentMusicTrack = musicName;
        
        if (fadeIn)
        {
            musicFadeCoroutine = StartCoroutine(FadeInMusic(clipToPlay, musicData));
        }
        else
        {
            PlayMusicImmediate(clipToPlay, musicData);
        }
    }
    
    public void StopMusic(bool fadeOut = true)
    {
        if (fadeOut)
        {
            if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = StartCoroutine(FadeOutMusic());
        }
        else
        {
            musicSource.Stop();
        }
        
        currentMusicTrack = "";
    }
    
    private void PlayMusicImmediate(AudioClip clip, SoundData musicData)
    {
        musicSource.clip = clip;
        musicSource.volume = musicData.volume;
        musicSource.pitch = musicData.pitch;
        musicSource.loop = musicData.loop;
        musicSource.Play();
        print($"playing {clip.name}");
    }
    
    private IEnumerator FadeInMusic(AudioClip clip, SoundData musicData)
    {
        // Fade out current music if playing
        if (musicSource.isPlaying)
        {
            float fadeOutTime = audioSettings.musicFadeDuration * 0.3f;
            float startVolume = musicSource.volume;
            
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / fadeOutTime;
                yield return null;
            }
        }
        
        // Set up new music
        musicSource.clip = clip;
        musicSource.volume = 0f;
        musicSource.pitch = musicData.pitch;
        musicSource.loop = musicData.loop;
        musicSource.Play();
        
        // Fade in new music
        float targetVolume = musicData.volume;
        while (musicSource.volume < targetVolume)
        {
            musicSource.volume += targetVolume * Time.deltaTime / audioSettings.musicFadeDuration;
            yield return null;
        }
        
        musicSource.volume = targetVolume;
    }
    
    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / audioSettings.musicFadeDuration;
            yield return null;
        }
        
        musicSource.Stop();
        musicSource.volume = startVolume;
    }
    
    #endregion
    
    #region SFX Management
    
    public void PlaySFX(string soundName, float volumeMultiplier = 1f)
    {
        if (!sounds.ContainsKey(soundName))
        {
            Debug.LogWarning($"SFX '{soundName}' not found in sound library");
            return;
        }
        
        SoundData soundData = sounds[soundName];
        AudioSource source = GetSFXSource();
        
        if (source != null)
        {
            PlaySoundOnSource(source, soundData, volumeMultiplier);
        }
    }
    
    private AudioSource GetSFXSource()
    {
        if (sfxSourcePool.Count > 0)
        {
            AudioSource source = sfxSourcePool.Dequeue();
            activeSfxSources.Add(source);
            return source;
        }
        
        // If no sources available, find one that's finished playing
        for (int i = activeSfxSources.Count - 1; i >= 0; i--)
        {
            if (!activeSfxSources[i].isPlaying)
            {
                AudioSource source = activeSfxSources[i];
                activeSfxSources.RemoveAt(i);
                return source;
            }
        }
        
        return null; // All sources busy
    }
    
    private void PlaySoundOnSource(AudioSource source, SoundData soundData, float volumeMultiplier)
    {
        AudioClip clip = soundData.GetRandomClip();
        if (clip == null) return;
        
        source.clip = clip;
        source.volume = (soundData.volume + Random.Range(-soundData.volumeVariance, soundData.volumeVariance)) * volumeMultiplier;
        source.pitch = soundData.pitch + Random.Range(-soundData.pitchVariance, soundData.pitchVariance);
        source.loop = soundData.loop;
        
        source.Play();
        
        if (!soundData.loop)
        {
            StartCoroutine(ReturnSourceToPool(source, clip.length));
        }
    }
    
    private IEnumerator ReturnSourceToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (activeSfxSources.Contains(source))
        {
            activeSfxSources.Remove(source);
            sfxSourcePool.Enqueue(source);
        }
    }
    
    private IEnumerator CleanupTempAudioSource(GameObject audioObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioObject != null) Destroy(audioObject);
    }
    
    #endregion
    
    #region Volume Controls
    
    public void SetMasterVolume(float volume)
    {
        float dbValue = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("MasterVolume", dbValue);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        float dbValue = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("MusicVolume", dbValue);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        float dbValue = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("SFXVolume", dbValue);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    
    private void LoadAudioSettings()
    {
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", audioSettings.defaultMasterVolume));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", audioSettings.defaultMusicVolume));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", audioSettings.defaultSFXVolume));
    }
    
    #endregion
    
    #region Scene Management
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Handle music transitions between scenes
        string sceneName = scene.name.ToLower();
        
        if (sceneName.Contains("menu") || sceneName.Contains("main"))
        {
            PlayMusic("menu_music");
        }
        else if (sceneName.Contains("game") || sceneName.Contains("play"))
        {
            PlayMusic("gameplay_music");
        }
    }
    
    private void StopAllAudio()
    {
        StopMusic(false);
        
        // Stop all active SFX
        foreach (var source in activeSfxSources)
        {
            if (source != null) source.Stop();
        }
    }
    
    #endregion
}