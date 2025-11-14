using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _gravityAcceleration;
    [SerializeField] private float _maxFallSpeed;
    [SerializeField] private float _maxForwardSpeed;
    [SerializeField] private float _maxBackwardSpeed;
    [SerializeField] private float _maxStrafeSpeed;
    [SerializeField] private float _jumpSpeed;
    [SerializeField] private float _standHeight;
    [SerializeField] private float _crouchHeight;
    [SerializeField] private float _crouchTransitionSpeed;
    [SerializeField] private float _maxLookUpAngle;
    [SerializeField] private float _maxLookDownAngle;

    private CharacterController _controller;
    private Transform           _head;
    private Vector3             _headRotation;
    private Vector3             _velocityHor;
    private Vector3             _velocityVer;
    private Vector3             _motion;
    private bool                _jump;
    private bool                _crouch;
    private Vector3 _cameraLocalPos;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _head       = GetComponentInChildren<Camera>().transform;
        _cameraLocalPos = _head.localPosition;
    }

    void Update()
    {
        UpdateRotation();
        UpdateHead();
        CheckForJump();
        CheckForCrouch();
        UpdateCameraHeight();
    }

    private void UpdateRotation()
    {
        float rotation = Input.GetAxis("Mouse X");

        transform.Rotate(0f, rotation, 0f);
    }

    private void UpdateHead()
    {
        _headRotation = _head.localEulerAngles;

        _headRotation.x -= Input.GetAxis("Mouse Y");

        if (_headRotation.x > 180f)
            _headRotation.x = Mathf.Max(_maxLookUpAngle, _headRotation.x);
        else
            _headRotation.x = Mathf.Min(_maxLookDownAngle, _headRotation.x);

        _head.localEulerAngles = _headRotation;
    }
    
    private void CheckForJump()
    {
        if (Input.GetButtonDown("Jump") && _controller.isGrounded)  
            _jump = true;
    }
    private void CheckForCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Debug.Log("Crouch");
            _crouch = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            Debug.Log("Stand Up");
            _crouch = false;
        }
    }

    void FixedUpdate()
    {
        UpdateVelocityHor();
        UpdateVelocityVer();
        UpdatePosition();
    }

    private void UpdateVelocityHor()
    {
        float forwardAxis   = Input.GetAxis("Forward");
        float strafeAxis    = Input.GetAxis("Strafe");

        if (forwardAxis >= 0f)
            _velocityHor.z = forwardAxis * _maxForwardSpeed;
        else
            _velocityHor.z = forwardAxis * _maxBackwardSpeed;

        _velocityHor.x = strafeAxis * _maxStrafeSpeed;

        if (_velocityHor.magnitude > _maxForwardSpeed)
            _velocityHor = _velocityHor.normalized * (forwardAxis > 0 ? _maxForwardSpeed : _maxBackwardSpeed);
    }

    private void UpdateVelocityVer()
    {
        if (_jump)
        {
            _velocityVer.y = _jumpSpeed;
            _jump = false;
        }
        else if (_controller.isGrounded)
            _velocityVer.y = -0.1f;
        else if (_velocityVer.y > -_maxFallSpeed)
        {
            _velocityVer.y += _gravityAcceleration * Time.fixedDeltaTime;
            _velocityVer.y = Mathf.Max(_velocityVer.y, -_maxFallSpeed);
        }
    }

    private void UpdatePosition()
    {
        _motion = (_velocityHor + _velocityVer) * Time.fixedDeltaTime;
        _motion = transform.TransformVector(_motion);

        _controller.Move(_motion);
    }
    private void UpdateCameraHeight()
    {
        float targetHeight = _crouch ? _crouchHeight : _standHeight;

        Vector3 newPos = _head.localPosition;
        newPos.y = Mathf.Lerp(newPos.y, targetHeight, Time.deltaTime * _crouchTransitionSpeed);

        _head.localPosition = newPos;
    }
}   