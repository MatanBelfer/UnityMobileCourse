using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Scriptable Objects/Sound Data")]
public class SoundData : ScriptableObject
{
    [Header("Audio Clips")]
    public AudioClip[] audioClips;
    
    [Header("Playback Settings")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    [Range(0f, 0.5f)] public float volumeVariance = 0.1f;
    [Range(0f, 0.5f)] public float pitchVariance = 0.1f;
    public bool loop = false;
    
    [Header("3D Audio")]
    public bool is3D = false;
    [Range(0f, 1f)] public float spatialBlend = 0f;
    public float minDistance = 1f;
    public float maxDistance = 20f;
    
    [Header("Mobile Performance")]
    public bool preload = true;
    public AudioClipLoadType loadType = AudioClipLoadType.DecompressOnLoad;
    
    public AudioClip GetRandomClip()
    {
        if (audioClips == null || audioClips.Length == 0) return null;
        return audioClips[Random.Range(0, audioClips.Length)];
    }
}