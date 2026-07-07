using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private SnakeGame snakeGame;

    [Header("Settings")]
    [SerializeField] private float minDuration = 3f;
    [SerializeField] private float maxDuration = 5f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Erratic Animation")]
    [SerializeField] private float jumpFrequency = 0.8f;
    [SerializeField] private float jumpAmount = 0.5f;
    [SerializeField] private float settleDuration = 0.5f;
    [SerializeField] private float smoothingSpeed = 1.5f;
    [SerializeField] private float stutterChance = 0.05f;
    [SerializeField] private AudioClip _ac;
    private AudioSource _as => GetComponent<AudioSource>();

    private float _targetProgress = 0f;
    private float _currentProgress = 0f;

    private static bool _hasLoaded = false;

    private void Start()
    {
        if (snakeGame != null)
            snakeGame.HideAll();

        if (_hasLoaded)
        {
            Debug.Log("Loading skipped – showing snake menu.");
            ShowSnakeMenu();
            return;
        }

        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        if (_ac != null && _as != null)
            _as.PlayOneShot(_ac);

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        _currentProgress = 0f;
        _targetProgress = 0f;
        if (progressBar != null)
            progressBar.value = 0f;

        float totalDuration = Random.Range(minDuration, maxDuration);
        float elapsed = 0f;

        while (_currentProgress < 0.95f)
        {
            elapsed += Time.deltaTime;
            float baseProgress = Mathf.Clamp01(elapsed / totalDuration);

            if (Random.value < stutterChance * Time.deltaTime * 10f)
            {
                if (Random.value < 0.5f)
                    _targetProgress = Mathf.Clamp01(_targetProgress - Random.Range(0.1f, 0.5f));
                else
                    _targetProgress = 0f;
            }
            else if (Random.value < jumpFrequency * Time.deltaTime * 2f)
            {
                float jump = Random.Range(-jumpAmount, jumpAmount);
                _targetProgress = Mathf.Clamp01(baseProgress + jump);
            }
            else
            {
                _targetProgress = Mathf.Lerp(_targetProgress, baseProgress, Time.deltaTime * 1.2f);
            }

            _currentProgress = Mathf.Lerp(_currentProgress, _targetProgress, Time.deltaTime * smoothingSpeed);
            _currentProgress = Mathf.Clamp01(_currentProgress);
            if (progressBar != null)
                progressBar.value = _currentProgress;

            yield return null;
        }

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


        ShowSnakeMenu();

        _hasLoaded = true;
        Debug.Log("Loading complete – snake menu shown.");
    }

    private void ShowSnakeMenu()
    {
        if (!snakeGame.gameObject.activeSelf)
        {
            Debug.Log("Activating SnakeGame GameObject");
            snakeGame.gameObject.SetActive(true);
        }

        Transform parent = snakeGame.transform.parent;
        while (parent != null)
        {
            if (!parent.gameObject.activeSelf)
            {
                Debug.Log($"Activating parent: {parent.name}");
                parent.gameObject.SetActive(true);
            }
            parent = parent.parent;
        }

        snakeGame.ShowMenu();
        Debug.Log("Snake menu shown successfully.");
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

    public void ResetLoadState()
    {
        _hasLoaded = false;
        Debug.Log("Load state reset.");
    }
}