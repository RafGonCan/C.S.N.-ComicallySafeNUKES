using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause_Menu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject areYouSure;
    [SerializeField] private PlayerMovement playerMovement;
    private bool _isPaused;
    public bool Paused => _isPaused;

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (_isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        _isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;

        playerMovement.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        _isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;

        playerMovement.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void YouSure()
    {
        areYouSure.SetActive(true);
    }
    public void NotSure()
    {
        areYouSure.SetActive(false);
    }
    public void Exit()
    {
        _isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
