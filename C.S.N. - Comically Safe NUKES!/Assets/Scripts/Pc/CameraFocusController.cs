using Unity.VisualScripting;
using UnityEngine;

public class CameraFocusController : MonoBehaviour
{
    private bool isFocusing;
    private bool isReturning;
    private Transform normalPosition;
    private Transform cam;
    private Transform targetFocusPoint;
    [SerializeField]private float movespeed;

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
            cam.position = 
                Vector3.Lerp(cam.position, targetFocusPoint.position, movespeed * Time.deltaTime);
            cam.rotation = 
                Quaternion.Lerp(cam.rotation, targetFocusPoint.rotation, movespeed * Time.deltaTime);
       }

        if (Vector3.Distance(cam.position, targetFocusPoint.position) < 0.01f)
        {
            cam.position = targetFocusPoint.position;
            cam.rotation = targetFocusPoint.rotation;
            isFocusing = false; // optional, depending on behavior
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
            }
        }

    }

    public void EnterFocus(Transform focusPoint)
    {
        if (isFocusing) return;

        isFocusing = true;
        isReturning = false;
        targetFocusPoint = focusPoint;
    }

    public void ExitFocus()
    {
        isFocusing = false;
        isReturning = true;
        targetFocusPoint = null;
    }

    public bool GetFocusing() => isFocusing;

}
