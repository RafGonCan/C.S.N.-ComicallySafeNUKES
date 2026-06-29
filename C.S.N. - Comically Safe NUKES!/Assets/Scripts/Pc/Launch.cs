using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class Launch : Interactive
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private string sceneToLoad;
    protected override void InteractSelf(bool direct)
    {
        base.InteractSelf(direct);
        playerMovement.enabled = false;
        SceneManager.LoadScene(sceneToLoad);
        Debug.Log("Launch InteractSelf called. PlayerMovement disabled and animation triggered.");
    }
}