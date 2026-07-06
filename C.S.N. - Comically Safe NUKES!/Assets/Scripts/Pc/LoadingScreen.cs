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

    [Header("Erratic Animation")]
    [SerializeField] private float jumpFrequency = 0.3f;
    [SerializeField] private float jumpAmount = 0.15f;
    [SerializeField] private float minProgress = 0.05f;

    private float _targetProgress = 0f;
    private float _currentProgress = 0f;
    private bool _isLoading = true;

    private void Start()
    {
        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
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


        while (_currentProgress < 1f)
        {
            elapsed += Time.deltaTime;

            float baseProgress = Mathf.Clamp01(elapsed / totalDuration);

            if (Random.value < jumpFrequency * Time.deltaTime * 2f)
            {
                float jump = Random.Range(-jumpAmount, jumpAmount);
                _targetProgress = Mathf.Clamp01(baseProgress + jump);
            }
            else
            {
                _targetProgress = Mathf.Lerp(_targetProgress, baseProgress, Time.deltaTime * 2f);
            }

            _currentProgress = Mathf.Lerp(_currentProgress, _targetProgress, Time.deltaTime * 3f);

            _currentProgress = Mathf.Clamp01(_currentProgress);
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

        _isLoading = false;
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
}