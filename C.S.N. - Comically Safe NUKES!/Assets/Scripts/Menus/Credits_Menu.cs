using System.Collections;
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
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }
}
