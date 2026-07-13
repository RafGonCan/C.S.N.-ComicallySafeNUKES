using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Launch : Interactive
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private FadeOuter fadeOuter;
    [SerializeField] private FluidScale fluidScale;

    protected override void InteractSelf(bool direct)
    {
        if (fluidScale.PlutoniumGet() >= 3)
        {
            base.InteractSelf(direct);

            if (playerMovement != null)
            playerMovement.enabled = false;

            fadeOuter.FadeToNextScene();
        }
    }
}