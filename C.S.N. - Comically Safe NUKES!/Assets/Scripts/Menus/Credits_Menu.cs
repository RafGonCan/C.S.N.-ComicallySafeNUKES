using UnityEngine;

public class Credits_Menu : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void OpenCreditsMenu()
    {
        gameObject.SetActive(true);
    }

    public void CloseCreditsMenu()
    {
        gameObject.SetActive(false);
    }
}
