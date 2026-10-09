using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class ParkingBay : MonoBehaviour
{
    [SerializeField] private Rigidbody _truckRigidbody;
    [SerializeField] private TruckMovement _truckMovement;

    [SerializeField] private float _maxParkingAngle = 15f;
    [SerializeField] private float _maxParkingSpeed = 0.5f;
    [SerializeField] private float _requiredHoldTime = 3f;

    public event Action<float> OnTimerUpdated;
    public event Action<float> OnProgressUpdated;
    public event Action<float> OnParked;
    public event Action OnGameReset;

    private Collider _bayCollider;
    private float _elapsedTime;
    private float _parkTimer;
    private bool _isParked;
    private bool _isTruckFullyInside;
    private TruckInput _truckInput;

    private void Awake()
    {
        _bayCollider = GetComponent<Collider>();
        _truckInput = new TruckInput();
        _truckInput.Move.Restart.performed += HandleRestartPerformed;
    }

    private void OnEnable()
    {
        _truckInput.Enable();
    }

    private void OnDisable()
    {
        _truckInput.Move.Restart.performed -= HandleRestartPerformed;
        _truckInput.Disable();
    }

    private void Start()
    {
        ResetGame();
    }

    private void Update()
    {
        if (_isParked) return;

        _elapsedTime += Time.deltaTime;
        OnTimerUpdated?.Invoke(_elapsedTime);

        if (IsParkingValid())
        {
            _parkTimer += Time.deltaTime;
            OnProgressUpdated?.Invoke(_parkTimer / _requiredHoldTime);

            if (_parkTimer >= _requiredHoldTime)
            {
                _isParked = true;
                OnParked?.Invoke(_elapsedTime);
            }
        }
        else
        {
            _parkTimer = 0f;
            OnProgressUpdated?.Invoke(0f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody == _truckRigidbody)
        {
            bool containsMin = _bayCollider.bounds.Contains(other.bounds.min);
            bool containsMax = _bayCollider.bounds.Contains(other.bounds.max);
            _isTruckFullyInside = containsMin && containsMax;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody == _truckRigidbody)
        {
            _isTruckFullyInside = false;
        }
    }

    private bool IsParkingValid()
    {
        if (!_isTruckFullyInside) return false;

        float angle = Vector3.Angle(transform.forward, _truckRigidbody.transform.forward);
        if (angle > _maxParkingAngle) return false;

        if (_truckRigidbody.linearVelocity.magnitude >= _maxParkingSpeed) return false;

        return true;
    }

    private void HandleRestartPerformed(InputAction.CallbackContext context)
    {
        ResetGame();
    }

    private void ResetGame()
    {
        _truckMovement.ResetPosition();
        _elapsedTime = 0f;
        _parkTimer = 0f;
        _isParked = false;
        _isTruckFullyInside = false;
        
        OnGameReset?.Invoke();
    }
}