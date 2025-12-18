using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class InspectionTool : MonoBehaviour
{
    [SerializeField] private float               _rotationSpeed = 100f;
    [SerializeField] private PlayerInventory     _playerInventory;
    public static event Action<bool, Interactive> OnInspectionStateChanged;

    private PlayerMovement      _playerMovement;
    private Camera              _mainCamera;
    private InspectionRoomData  _inspectionRoom;
    private Coroutine           _scaleCoroutine;
    private float               _scaleDuration = 0.5f;
    private Interactive         _currentInspect;
    private GameObject          _spawnedInspectObject;
    private GameObject          _currentInspectionModel;
    private StatefulInteractive _currentStatefulInspection;
    
    private bool                _stillInAnimation;
    public bool                 isInspecting = false;
    public bool                 IsInspecting => isInspecting;

    private void Start()
    {
        _playerMovement = _playerInventory.GetComponent<PlayerMovement>();        
        _mainCamera = _playerInventory.GetComponentInChildren<Camera>();
        _inspectionRoom = GetComponent<InspectionRoomData>();
    }
    private void Update()
    {
        if (isInspecting)
        {
            HandleInspectionInput();
        
            if (Input.GetMouseButtonDown(0) && _currentStatefulInspection != null)
            {
                CheckForPartInteraction();
            }
        }
    }
    /// <summary>
    /// WIP  
    /// </summary>
    private void CheckForPartInteraction()
    {
        if (_currentInspectionModel == null) return;
    
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
    
        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (hit.transform.IsChildOf(_currentInspectionModel.transform))
            {
                GameObject clickedObject = hit.transform.gameObject;
            
                if (_currentStatefulInspection != null)
                {
                    int partIndex;
                    if (_currentStatefulInspection.IsToggleablePart(clickedObject, out partIndex))
                    {
                        _currentStatefulInspection.TogglePart(partIndex);
                        return;
                    }
                }
            
                Transform parent = hit.transform.parent;
                while (parent != null && parent != _currentInspectionModel.transform)
                {
                    if (_currentStatefulInspection != null)
                    {
                        int partIndex;
                        if (_currentStatefulInspection.IsToggleablePart(parent.gameObject, out partIndex))
                        {
                            _currentStatefulInspection.TogglePart(partIndex);
                            return;
                        }
                    }
                    parent = parent.parent;
                }
            }
        }
    }
    /// <summary>
    /// Inspection tool starting method
    /// </summary>
    /// <param name="item"></param>
    public void StartInspection(Interactive item)
    {
        OnInspectionStateChanged?.Invoke(true, item);

        if (isInspecting || item == null) return;
    
        _currentInspect = item;

        //Based on the inventory item selected, inspects it by creating the model
        _currentInspectionModel = item.CreateInspectionModel(); 
        if (_currentInspectionModel == null)
        {
            Debug.LogWarning("Failed to create inspection model for: " + item.name);
            return;
        }
        if (_inspectionRoom != null)
        {
            // Gets the position/rotation of the inspection room to set as position/rotation 
            // of the inspected object
            _currentInspectionModel.transform.SetParent(_inspectionRoom.transform);
            _currentInspectionModel.transform.localPosition = _inspectionRoom.ObjectPosition;
            _currentInspectionModel.transform.localEulerAngles = _inspectionRoom.ObjectRotation;
        }
        else
        {
            // In case _inspectionRoom is null, it will use the mainCamera as default for positioning the item
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _currentInspectionModel.transform.position = mainCamera.transform.position + 
                    mainCamera.transform.forward * 1.5f;
                _currentInspectionModel.transform.LookAt(mainCamera.transform);
                _currentInspectionModel.transform.Rotate(0, 180, 0);
            }
        }
        

        isInspecting = true;
        _currentInspectionModel.transform.localScale = Vector3.zero;

    
        _currentStatefulInspection = _currentInspectionModel.GetComponent<StatefulInteractive>();
        if (_currentStatefulInspection != null)
        {
            _currentStatefulInspection.SetupForInspection();
        }

        SwitchToInspectionCamera(true);
        
        if (_scaleCoroutine != null)
            StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScaleInAnimation());
        _playerMovement.SetControlsEnabled(false);
    }
    /// <summary>
    /// Animation for the "pop in" effect of the item when created
    /// </summary>
    /// <returns></returns>
    private IEnumerator ScaleInAnimation()
    {
        _stillInAnimation = true;
        if (_currentInspectionModel == null) yield break;
        
        float elapsedTime = 0f;
        Vector3 targetScale = Vector3.one * 0.5f;
        
        while (elapsedTime < _scaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _scaleDuration);
            
            t = 1f - Mathf.Pow(1f - t, 3);
            
            _currentInspectionModel.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }
        
        _currentInspectionModel.transform.localScale = targetScale;
        _scaleCoroutine = null;
        _stillInAnimation = false;
    }
    /// <summary>
    /// Disables main camera and enables the inspection room camera (true for inspection camera on, false for off)
    /// </summary>
    /// <param name="toInspection"></param>
    private void SwitchToInspectionCamera(bool toInspection)
    {
        if (_inspectionRoom.InspectionCamera == null || _mainCamera == null) return;

        if (toInspection)
        {
            _mainCamera.enabled = false;

            _inspectionRoom.InspectionCamera.enabled = true;            
            _inspectionRoom.InspectionCamera.nearClipPlane = 0.1f;
            _inspectionRoom.InspectionCamera.farClipPlane = 10f;
        }
        else
        {
            _mainCamera.enabled = true;
            _inspectionRoom.InspectionCamera.enabled = false;
        }
    }
    /// <summary>
    /// Collects player input for object rotation or leaving inspect
    /// </summary>
    private void HandleInspectionInput()
    {
        if (!isInspecting) return;

        // Leaving inspect
        if (Input.GetKeyDown(KeyCode.E) && !_stillInAnimation)
        {
            EndInspection();
            return;
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
            
        // Rotate the object with Mouse1
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * _rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * _rotationSpeed;
                
            if (_currentInspectionModel != null)
            {
                _currentInspectionModel.transform.Rotate(Vector3.up, -mouseX, Space.World);
                _currentInspectionModel.transform.Rotate(Vector3.right, mouseY, Space.World);
            }
        }
    }
    /// <summary>
    /// Method for changing inspection state to false
    /// </summary>
    public void EndInspection()
    {
        OnInspectionStateChanged?.Invoke(false, null);

              
        
        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = null;
        }
        // Coroutine for "pop out" effect of the object when the inspect ends
        StartCoroutine(ScaleOutAndEnd());
        
    }
    /// <summary>
    /// Animation for the "pop out" effect of the item when leaving inspect tool
    /// </summary>
    /// <returns></returns>
    private IEnumerator ScaleOutAndEnd()
    {
        yield return null;

        if (_currentInspectionModel != null)
        {
            float elapsedTime = 0f;
            Vector3 startScale = _currentInspectionModel.transform.localScale;
            
            while (elapsedTime < _scaleDuration * 0.5f)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / (_scaleDuration * 0.5f));
                
                t = t * t;

                _currentInspectionModel.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

                yield return null;
            }
        }
        SwitchToInspectionCamera(false);
        CleanupAfterScaleOut();
    }
    /// <summary>
    /// WIP
    /// </summary>
    private void CleanupAfterScaleOut()
    {
        if (_currentInspect != null && _currentInspectionModel != null)
        {
            _currentInspect.UpdateFromInspectionModel(_currentInspectionModel);
            
            if (_currentStatefulInspection != null)
            {
                _currentStatefulInspection.CleanupAfterInspection();
            }
        }
        
        if (_currentInspectionModel != null)
        {
            Destroy(_currentInspectionModel);
            _currentInspectionModel = null;
        }
        
        if (_spawnedInspectObject != null)
        {
            Destroy(_spawnedInspectObject);
            _spawnedInspectObject = null;
        }

        isInspecting = false;
        _currentInspect = null;
        _currentStatefulInspection = null;
        
        _playerMovement.SetControlsEnabled(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
