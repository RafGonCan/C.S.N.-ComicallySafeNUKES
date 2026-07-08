using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class CameraFocusController : MonoBehaviour
{
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject leaveFocus;
    [SerializeField] private UIManager _uiManager;
    private Interactive _focusedInteractive;
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

    private InputSystem_Actions _inputActions;
    private InputAction _cancelAction;
    private bool _uiReady = false;
    private Coroutine _uiReadyCoroutine;
    private bool _allowCancelExit = true;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();
        _cancelAction = _inputActions.UI.Cancel;
        _cancelAction.performed += OnCancel;
    }

    private void Start()
    {
        cam = transform;
        normalPosition = new GameObject("CameraNormalPoint").transform;
        normalPosition.SetParent(cam.transform.parent);
        normalPosition.localPosition = cam.transform.localPosition;
        normalPosition.localRotation = cam.transform.localRotation;
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _cancelAction.performed -= OnCancel;
            _inputActions.Disable();
        }
    }

    private void LateUpdate()
    {
        if (isFocusing && targetFocusPoint != null)
        {
            playermovement.enabled = false;

            cam.position = Vector3.Lerp(cam.position, targetFocusPoint.position, movespeed * Time.deltaTime);
            cam.rotation = Quaternion.Lerp(cam.rotation, targetFocusPoint.rotation, movespeed * Time.deltaTime);

            if (Vector3.Distance(cam.position, targetFocusPoint.position) < 0.01f)
            {
                cam.position = targetFocusPoint.position;
                cam.rotation = targetFocusPoint.rotation;
                InteractionManager.instance.UpdateCursorState();
            }
        }

        if (isReturning)
        {
            cam.position = Vector3.Lerp(cam.position, normalPosition.position, movespeed * Time.deltaTime);
            cam.rotation = Quaternion.Lerp(cam.rotation, normalPosition.rotation, movespeed * Time.deltaTime);

            if (Vector3.Distance(cam.position, normalPosition.position) < 0.01f)
            {
                isReturning = false;
                cam.localPosition = normalPosition.localPosition;
                cam.localRotation = normalPosition.localRotation;
                playermovement.enabled = true;
                InteractionManager.instance.UpdateCursorState();
            }
        }
    }

    public void EnterFocus(Transform focusPoint, Interactive interactive, bool allowCancelExit = true)
    {
        if (isFocusing) return;
        _uiManager.ShowInventory(false);

        crosshair.SetActive(false);
        _focusedInteractive = interactive;
        _uiReady = false;
        _allowCancelExit = allowCancelExit;

        InteractionManager.instance.SetCursorAllowed(true);

        leaveFocus.SetActive(allowCancelExit);

        blockExitFocus = true;

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

            if (_uiReadyCoroutine != null)
                StopCoroutine(_uiReadyCoroutine);
            _uiReadyCoroutine = StartCoroutine(EnableUIAfterDelay());
        }
        else
        {
            Debug.LogWarning($"No World Space Canvas found under {focusPoint.name}");
        }
    }

    private IEnumerator EnableUIAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        _uiReady = true;
        _uiReadyCoroutine = null;

        if (currentCanvas != null)
        {
            Selectable firstSelectable = currentCanvas.GetComponentInChildren<Selectable>();
            if (firstSelectable != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
                Debug.Log($"Selected UI element: {firstSelectable.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("No selectable UI element found in the canvas.");
            }
        }
    }

    public void ExitFocus()
    {     
        if (blockExitFocus) return;
        _uiManager.ShowInventory(true);
        isFocusing = false;
        isReturning = true;
        targetFocusPoint = null;
        crosshair.SetActive(true);
        leaveFocus.SetActive(false);
        _uiReady = false;
        if (_uiReadyCoroutine != null)
        {
            StopCoroutine(_uiReadyCoroutine);
            _uiReadyCoroutine = null;
        }

        InteractionManager.instance.SetCursorAllowed(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (currentCanvas != null)
        {
            currentCanvas.gameObject.SetActive(false);
            currentCanvas = null;
        }

        if (_focusedInteractive != null)
        {
            _focusedInteractive.RestoreCollider();
            _focusedInteractive = null;
        }
    }

    public void ExitButton()
    {
        blockExitFocus = false;
        ExitFocus();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (isFocusing && _uiReady && _allowCancelExit)
        {
            blockExitFocus = false;
            ExitFocus();
        }
    }

    public bool GetFocusing() => isFocusing;
    public bool GetReturning() => isReturning;
}