using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    private List<AudioSource> activeAudioSources = new List<AudioSource>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StopAllSounds()
    {
        foreach (AudioSource audioSource in activeAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        activeAudioSources.Clear();
    }

    public void RegisterActiveSound(AudioSource audioSource)
    {
        if (!activeAudioSources.Contains(audioSource))
        {
            activeAudioSources.Add(audioSource);
        }
    }

    public void UnregisterActiveSound(AudioSource audioSource)
    {
        activeAudioSources.Remove(audioSource);
    }
}