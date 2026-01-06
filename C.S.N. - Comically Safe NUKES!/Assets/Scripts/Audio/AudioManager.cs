using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
    }
}
