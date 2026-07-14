using System.Collections;
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
        HideText();
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
            List<(float, float, string)> parsedLines = ParseTrack(_trackData[i]);
            subtitleDict.Add(key, parsedLines);
        }
    }

    private List<(float startTime, float endTime, string text)> ParseTrack(string s)
    {
        List<(float, float, string)> result = new List<(float, float, string)>();
        if (string.IsNullOrEmpty(s)) return result;

        float cursor = 0f;
        string[] rows = s.Split('\n');
        foreach (string sRow in rows)
        {
            string row = sRow.Trim('\r', '\n', ' ');
            if (string.IsNullOrWhiteSpace(row)) continue;

            string[] parts = row.Split('|');
            if (parts.Length < 3) continue;

            if (!float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float duration))
                continue;

            float start = cursor;
            float end = start + duration;
            result.Add((start, end, parts[2].Trim()));
            cursor = end;
        }

        return result;
    }

    public void PlaySubtitles(string key, AudioSource source, float clipLength)
    {
        StopSubtitles();

        bool found = subtitleDict != null && subtitleDict.ContainsKey(key);
        if (string.IsNullOrEmpty(key) || subtitleDict == null || !subtitleDict.ContainsKey(key))
        {
            return;
        }

        subtitleCoroutine = StartCoroutine(RunSubtitles(subtitleDict[key], source, clipLength));
    }

    private IEnumerator RunSubtitles(List<(float startTime, float endTime, string text)> lines, AudioSource source, float clipLength)
    {
        float elapsed = 0f;

        foreach (var line in lines)
        {
            while (elapsed < line.startTime)
            {
                if (source == null || !source.isPlaying)
                {
                    HideText();
                    yield break;
                }
                yield return null;
                elapsed += Time.deltaTime;
            }

            ShowText(line.text);

            float lineEnd = Mathf.Min(line.endTime, clipLength);
            while (elapsed < lineEnd)
            {
                if (source == null || !source.isPlaying)
                {
                    HideText();
                    yield break;
                }
                yield return null;
                elapsed += Time.deltaTime;
            }

            HideText();
        }
        subtitleCoroutine = null;
    }

    public void StopSubtitles()
    {
        if (subtitleCoroutine != null)
        {
            StopCoroutine(subtitleCoroutine);
            subtitleCoroutine = null;
        }
        HideText();
    }

    private void ShowText(string text)
    {
        if (subtitleText == null) return;
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