using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
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
        DictionaryBuilder();
    }

    private void DictionaryBuilder()
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

            subtitleDict.Add(key, ParseTrack(_trackData[i]));
        }
    }

    private List<(float startTime, float endTime, string text)> ParseTrack(string s)
    {
        List<(float, float, string)> result = new List<(float, float, string)>();

        if (string.IsNullOrEmpty(s))
        {
            return result;
        }

        string[] rows = s.Split('\n');
        foreach (string sRow in rows)
        {
            string row = sRow.Trim('\r', '\n', ' ');
            if (string.IsNullOrWhiteSpace(row))
            {
                continue;
            }

            string[] parts = row.Split('|');
            if (parts.Length < 3)
            {
                continue;
            }

            if (!float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float start))
            {
                continue;
            }

            if (!float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float duration))
            {
                continue;
            }

            result.Add((start, start + duration, parts[2].Trim()));
        }

        result.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        return result;
    }

    private void ShowText(string text)
    {
        if (subtitleCoroutine != null) return;
        subtitleText.text = text;
        subtitleText.gameObject.SetActive(true);
    }

    private void HideText()
    {
        if (subtitleText == null) return;
        subtitleText.gameObject.SetActive(false);
        subtitleText.text = string.Empty;
    }

}
