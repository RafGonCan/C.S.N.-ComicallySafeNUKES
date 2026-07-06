using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Settings")]
    [SerializeField] private float minDuration = 3f;
    [SerializeField] private float maxDuration = 5f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Erratic Animation (very chaotic)")]
    [SerializeField] private float jumpFrequency = 0.8f;        // how often jumps occur (higher = more jumps)
    [SerializeField] private float jumpAmount = 0.5f;            // max jump size (can be negative)
    [SerializeField] private float settleDuration = 0.5f;
    [SerializeField] private float smoothingSpeed = 1.5f;        // lower = slower to follow, more smooth; higher = more jagged
    [SerializeField] private float stutterChance = 0.05f;        // chance per frame to freeze or jump back to 0

    [SerializeField] private AudioClip _bootUp;
    private AudioSource _as => GetComponent<AudioSource>();
    private float _targetProgress = 0f;
    private float _currentProgress = 0f;
    public bool LoadingComplete { get; private set; } = false;

    private void Start()
    {
        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        if (!LoadingComplete && _as != null && _bootUp != null)
            _as.PlayOneShot(_bootUp);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        _currentProgress = 0f;
        _targetProgress = 0f;
        if (progressBar != null)
            progressBar.value = 0f;

        float totalDuration = Random.Range(minDuration, maxDuration);
        float elapsed = 0f;

        // ----- Chaotic phase -----
        while (_currentProgress < 0.95f)
        {
            elapsed += Time.deltaTime;

            // Base progress (overall increase)
            float baseProgress = Mathf.Clamp01(elapsed / totalDuration);

            // ----- Stutter: freeze or drop to 0 -----
            if (Random.value < stutterChance * Time.deltaTime * 10f)
            {
                // Freeze for a moment or drop to zero
                if (Random.value < 0.5f)
                {
                    _targetProgress = Mathf.Clamp01(_targetProgress - Random.Range(0.1f, 0.5f));
                }
                else
                {
                    _targetProgress = 0f;
                }
            }
            // ----- Normal erratic jump -----
            else if (Random.value < jumpFrequency * Time.deltaTime * 2f)
            {
                float jump = Random.Range(-jumpAmount, jumpAmount);
                _targetProgress = Mathf.Clamp01(baseProgress + jump);
            }
            else
            {
                // Slow drift toward base, but with some inertia
                _targetProgress = Mathf.Lerp(_targetProgress, baseProgress, Time.deltaTime * 1.2f);
            }

            // Move current toward target with **low** smoothing (so it's more jagged)
            _currentProgress = Mathf.Lerp(_currentProgress, _targetProgress, Time.deltaTime * smoothingSpeed);

            // Clamp and update UI
            _currentProgress = Mathf.Clamp01(_currentProgress);
            if (progressBar != null)
                progressBar.value = _currentProgress;

            yield return null;
        }

        // ----- Settle phase: smoothly reach 1.0 -----
        float settleElapsed = 0f;
        float startProgress = _currentProgress;
        while (settleElapsed < settleDuration)
        {
            settleElapsed += Time.deltaTime;
            float t = settleElapsed / settleDuration;
            _currentProgress = Mathf.Lerp(startProgress, 1f, t);
            if (progressBar != null)
                progressBar.value = _currentProgress;
            yield return null;
        }

        if (progressBar != null)
            progressBar.value = 1f;

        yield return FadeToBlack();

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        EventSystem.current?.SetSelectedGameObject(
            mainMenuPanel.GetComponentInChildren<Button>()?.gameObject
        );

        LoadingComplete = true;
        Debug.Log("Loading complete – main menu shown.");
    }

    private IEnumerator FadeToBlack()
    {
        CanvasGroup cg = loadingPanel?.GetComponent<CanvasGroup>();
        if (cg == null) yield break;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
    }

    public void RestartLoading()
    {
        LoadingComplete = false;
        StartCoroutine(LoadingRoutine());
    }
}