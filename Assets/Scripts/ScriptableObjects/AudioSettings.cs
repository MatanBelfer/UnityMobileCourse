using UnityEngine;

[CreateAssetMenu(fileName = "AudioSettings", menuName = "Scriptable Objects/AudioSettings")]
public class AudioSettings : ScriptableObject
{
    [Header("Volume Defaults")]
    [Range(0f, 1f)] public float defaultMasterVolume = 0.8f;
    [Range(0f, 1f)] public float defaultMusicVolume = 0.6f;
    [Range(0f, 1f)] public float defaultSFXVolume = 0.7f;
    
    [Header("Fade Settings")]
    public float musicFadeDuration = 1.5f;
    public float sceneTransitionFadeTime = 0.5f;
    
    [Header("Performance")]
    public int sfxPoolSize = 8;
    public int maxSimultaneousAudioSources = 32;
    
    [Header("Mobile Optimization")]
    public bool useAudioCompression = true;
    public AudioCompressionFormat preferredCompressionFormat = AudioCompressionFormat.Vorbis;
}