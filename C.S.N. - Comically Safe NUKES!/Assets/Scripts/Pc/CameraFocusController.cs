using Unity.VisualScripting;
using UnityEngine;

public class CameraFocusController : MonoBehaviour
{
    private bool isFocusing;
    private bool isReturning;
    private Transform normalPosition;
    private Transform cam;
    private Transform targetFocusPoint;
    private Canvas currentCanvas;
    private bool blockExitFocus = true;
    public bool BlockExitFocus
    {
        get => blockExitFocus;
        set => blockExitFocus = value;
    }

    [SerializeField] private float movespeed;
    [SerializeField] private PlayerMovement playermovement;

    private void Start()
    {
        cam = transform;
        normalPosition = new GameObject("CameraNormalPoint").transform;
        normalPosition.SetParent(cam.transform.parent);
        normalPosition.localPosition = cam.transform.localPosition;
        normalPosition.localRotation = cam.transform.localRotation;
    }
    private void LateUpdate()
    {
       if (isFocusing && targetFocusPoint != null)
       {
            playermovement.enabled = false;

            cam.position = 
                Vector3.Lerp(cam.position, targetFocusPoint.position, movespeed * Time.deltaTime);
            cam.rotation = 
                Quaternion.Lerp(cam.rotation, targetFocusPoint.rotation, movespeed * Time.deltaTime);

            if (Vector3.Distance(cam.position, targetFocusPoint.position) < 0.01f)
            {
                cam.position = targetFocusPoint.position;
                cam.rotation = targetFocusPoint.rotation;

                EnableMouse(true);
            }
       }

       if (isReturning)
       {
           cam.position = 
               Vector3.Lerp(cam.position, normalPosition.position, movespeed * Time.deltaTime);
           cam.rotation = 
               Quaternion.Lerp(cam.rotation, normalPosition.rotation, movespeed * Time.deltaTime);

           if (Vector3.Distance(cam.position, normalPosition.position)<0.01f)
           {
               isReturning = false;

               cam.localPosition = normalPosition.localPosition;
               cam.localRotation = normalPosition.localRotation;

               playermovement.enabled = true;
               EnableMouse(false);
           }
       }

    }
    public void EnterFocus(Transform focusPoint)
    {
        if (isFocusing) return;

        if (currentCanvas != null)
        {
            currentCanvas.gameObject.SetActive(false);
            currentCanvas = null;
        }

        isFocusing = true;
        isReturning = false;
        targetFocusPoint = focusPoint;

        currentCanvas = focusPoint.GetComponentInChildren<Canvas>(true);

        if (currentCanvas != null && currentCanvas.renderMode == RenderMode.WorldSpace)
        {
            currentCanvas.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                $"No World Space Canvas found under {focusPoint.name}");
        }
    }

    public void ExitFocus()
    {
        if (blockExitFocus) return;
        isFocusing = false;
        isReturning = true;
        targetFocusPoint = null;

        if (currentCanvas != null)
        {
            currentCanvas.gameObject.SetActive(false);
            currentCanvas = null;
        }
    }
    public void ExitButton()
    {
        blockExitFocus = false;
        ExitFocus();
    }

    private void EnableMouse(bool enable)
    {
        Cursor.visible = enable;
        Cursor.lockState = enable ? CursorLockMode.None : CursorLockMode.Locked;    
    }

    public bool GetFocusing() => isFocusing;
    public bool GetReturning() => isReturning;

}
