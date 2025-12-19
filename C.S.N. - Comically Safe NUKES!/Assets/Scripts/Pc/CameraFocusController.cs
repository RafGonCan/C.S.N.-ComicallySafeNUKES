using Unity.VisualScripting;
using UnityEngine;

public class CameraFocusController : MonoBehaviour
{
    private bool isFocusing;
    private bool isReturning;
    private Transform normalPosition;
    private Transform cam;
    private Transform targetFocousPoint;
    [SerializeField]private float movespeed;

    private void Start()
    {
        cam = transform;
        normalPosition = new GameObject("CameraNormalPoint").transform;
        normalPosition.SetParent(cam.transform.parent);
        normalPosition.localPosition = cam.transform.localPosition;
        normalPosition.localRotation = cam.transform.localRotation;
    }
    private void Update()
    {
       if (isFocusing && targetFocousPoint != null)
       {
            cam.position = 
                Vector3.Lerp(cam.position, targetFocousPoint.position, movespeed * Time.deltaTime);
            cam.rotation = 
                Quaternion.Lerp(cam.rotation, targetFocousPoint.rotation, movespeed * Time.deltaTime);
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
        isFocusing = true;
        isReturning = false;
        targetFocousPoint = focusPoint;
    }

    public void ExitFocus()
    {
        isFocusing = false;
        isReturning = true;
        targetFocousPoint = null;
    }

    public bool GetFocusing() => isFocusing;

}
