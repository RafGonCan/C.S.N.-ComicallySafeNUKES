using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance;

    [SerializeField] private TMP_Text subtitleText;

    [SerializeField] private List<string> _trackKeys = new List<string>();

    [SerializeField, TextArea(3, 15)] private List<string> _trackData = new List<string>();

    private Dictionary<string, List<(float startTime, float endTime, string text)>> subtitleDict;
    private Coroutine subtitleCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void DisctionaryBuilder()
    {
        subtitleDict = new Dictionary<string, List<(float, float, string)>>();

        int count = Mathf.Min(_trackKeys.Count, _trackData.Count);
        for (int i = 0; i < count; i++)
        {
            string key = _trackKeys[i];
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            if (subtitleDict.ContainsKey(key))
            {
                continue;
            }

            subtitleDict.Add(key, new List<(float, float, string)>());
        }
    }

}
