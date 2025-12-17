using UnityEditor.ShaderGraph;
using UnityEngine;

public class PcScreen : MonoBehaviour
{
    private bool isFocusing;
    private bool isReturning;
    private Transform normalPosition;
    private Camera cam = Camera.main;
    private Transform targetFocousPoint;
    private float movespeed;

    private void Start()
    {
        normalPosition = new GameObject("CameraNormalPoint").transform;
        normalPosition.SetParent(cam.transform.parent);
        normalPosition.localPosition = cam.transform.localPosition;
        normalPosition.localRotation = cam.transform.localRotation;
    }
    private void Update()
    {
        if (isFocusing && targetFocousPoint != null)
        {
            cam.transform.position = 
                Vector3.Lerp(cam.transform.position, targetFocousPoint.position,movespeed * Time.deltaTime);
            cam.transform.rotation = 
                Quaternion.Lerp(cam.transform.rotation, targetFocousPoint.rotation,movespeed * Time.deltaTime);
            if (Vector3.Distance(cam.transform.position, normalPosition.position)<0.01f)
            {
                isReturning = false;

                cam.transform.localPosition = normalPosition.localPosition;
                cam.transform.localRotation = normalPosition.localRotation;
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

}
