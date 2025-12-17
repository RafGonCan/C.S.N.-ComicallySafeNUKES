using UnityEngine;

public class InspectionRoomData : MonoBehaviour
{
    [SerializeField] private Camera _inspectionCamera;
    [SerializeField] private Vector3 _objectPosition = new Vector3(0, 0, 2);
    [SerializeField] private Vector3 _objectRotation = Vector3.zero;
    public Transform InspectionRoom => transform;
    public Camera InspectionCamera => _inspectionCamera;
    public Vector3 ObjectPosition => _objectPosition;
    public Vector3 ObjectRotation => _objectRotation;
    
    void Awake()
    {
        if (_inspectionCamera == null)
            _inspectionCamera = GetComponentInChildren<Camera>();

        if (_inspectionCamera != null)
            _inspectionCamera.enabled = false;
        else
            Debug.LogError("No Camera found as child of InspectionRoom!");
    }
}