using UnityEngine;

public class InspectionRoomData : MonoBehaviour
{
    [SerializeField] private Camera _inspectionCamera;
    [SerializeField] private Vector3 _objectPosition = new Vector3(0, 0, 2);
    [SerializeField] private Vector3 _objectRotation = Vector3.zero;
    [SerializeField] private float _cameraFOV = 60f;
    [SerializeField] private float _nearClipPlane = 0.1f;
    [SerializeField] private float _farClipPlane = 10f;
    public Transform InspectionRoom => transform;
    public Camera InspectionCamera => _inspectionCamera;
    public Vector3 ObjectPosition => _objectPosition;
    public Vector3 ObjectRotation => _objectRotation;
    public float CameraFOV => _cameraFOV;
    public float NearClipPlane => _nearClipPlane;
    public float FarClipPlane => _farClipPlane;
    
    void Awake()
    {
        if (_inspectionCamera == null)
            _inspectionCamera = GetComponentInChildren<Camera>();

        if (_inspectionCamera != null)
            _inspectionCamera.enabled = false;
        else
            Debug.LogError("No Camera found as child of InspectionRoom!");
    }
    
    public void SetupCamera()
    {
        if (_inspectionCamera != null)
        {
            _inspectionCamera.fieldOfView = _cameraFOV;
            _inspectionCamera.nearClipPlane = _nearClipPlane;
            _inspectionCamera.farClipPlane = _farClipPlane;
        }
    }
}