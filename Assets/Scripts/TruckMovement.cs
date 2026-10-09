using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class TruckMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _turnSpeed = 100f;

    private Rigidbody _rigidbody;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    
    private TruckInput _truckInput;
    private Vector2 _moveInput;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _startPosition = transform.position;
        _startRotation = transform.rotation;

        _truckInput = new TruckInput();
    }

    private void OnEnable()
    {
        _truckInput.Move.WASD.performed += HandleMovePerformed;
        _truckInput.Move.WASD.canceled += HandleMoveCanceled;
        _truckInput.Enable();
    }

    private void OnDisable()
    {
        _truckInput.Move.WASD.performed -= HandleMovePerformed;
        _truckInput.Move.WASD.canceled -= HandleMoveCanceled;
        _truckInput.Disable();
    }

    private void FixedUpdate()
    {
        Vector3 move = transform.forward * (_moveInput.y * _moveSpeed * Time.fixedDeltaTime);
        _rigidbody.MovePosition(_rigidbody.position + move);

        if (Mathf.Abs(_moveInput.y) > 0.01f)
        {
            float direction = Mathf.Sign(_moveInput.y);
            Quaternion turn = Quaternion.Euler(0f, _moveInput.x * direction * _turnSpeed * Time.fixedDeltaTime, 0f);
            _rigidbody.MoveRotation(_rigidbody.rotation * turn);
        }
    }

    private void HandleMovePerformed(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void HandleMoveCanceled(InputAction.CallbackContext context)
    {
        _moveInput = Vector2.zero;
    }

    public void ResetPosition()
    {
        _rigidbody.position = _startPosition;
        _rigidbody.rotation = _startRotation;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _moveInput = Vector2.zero; // Resets residual input
    }
}