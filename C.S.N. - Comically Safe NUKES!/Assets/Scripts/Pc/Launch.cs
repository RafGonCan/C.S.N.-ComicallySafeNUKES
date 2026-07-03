using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Launch : Interactive
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private FadeOuter fadeOuter;

    protected override void InteractSelf(bool direct)
    {
        base.InteractSelf(direct);

        if (playerMovement != null)
            playerMovement.enabled = false;

        fadeOuter.FadeToNextScene();
    }
}