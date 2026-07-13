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

}
