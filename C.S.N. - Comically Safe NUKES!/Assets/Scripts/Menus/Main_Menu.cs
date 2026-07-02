using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu : MonoBehaviour
{
    [SerializeField] private GameObject blackScreen;
    [SerializeField] private GameObject speakerIcon;
    [SerializeField] private GameObject transitionObject;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float timeOnScreen = 3f;
    [SerializeField] private float timeToFade = 1f;

    private void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transitionObject.SetActive(true);
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        yield return StartCoroutine(FadeCanvasGroup(blackScreen, 0f, 1f, fadeSpeed));

        yield return StartCoroutine(FadeCanvasGroup(speakerIcon, 0f, 1f, fadeSpeed));

        yield return new WaitForSeconds(timeOnScreen);

        float fadeOutSpeed = 1f / timeToFade;
        yield return StartCoroutine(FadeCanvasGroup(speakerIcon, 1f, 0f, fadeOutSpeed));

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    private IEnumerator FadeCanvasGroup(GameObject obj, float startAlpha, float endAlpha, float speed)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            Debug.LogError("GameObject " + obj.name + " does not have a CanvasGroup component!");
            yield break;
        }

        cg.alpha = startAlpha;
        float elapsed = 0f;
        float duration = Mathf.Abs(endAlpha - startAlpha) / speed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        cg.alpha = endAlpha;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}