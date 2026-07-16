using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Main_Menu : MonoBehaviour
{
    [SerializeField] private GameObject[] cameras;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject blackScreen;
    [SerializeField] private GameObject speakerIcon;
    [SerializeField] private GameObject transitionObject;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float timeOnScreen = 3f;
    [SerializeField] private float timeToFade = 1f;

    private InputSystem_Actions _inputActions;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();
    }

    private void Start()
    {
        // Enable cursor for menu
        InteractionManager.instance.SetCursorAllowed(true);

        if (EventSystem.current != null && startButton != null)
            EventSystem.current.SetSelectedGameObject(startButton);
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
            _inputActions.Disable();
    }

    public void StartGame()
    {
        // Hide cursor for gameplay
        InteractionManager.instance.SetCursorAllowed(false);
        EventSystem.current.SetSelectedGameObject(null);

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

    public void SettingsCamera()
    {
        cameras[0].SetActive(false);
        cameras[1].SetActive(true);
        cameras[2].SetActive(false);
    }

    public void CreditsCamera()
    {
        cameras[0].SetActive(false);
        cameras[1].SetActive(false);
        cameras[2].SetActive(true);
    }

    public void MenuCamera()
    {
        cameras[0].SetActive(true);
        cameras[1].SetActive(false);
        cameras[2].SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}