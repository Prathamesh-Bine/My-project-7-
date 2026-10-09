using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeliveryBay : MonoBehaviour
{
    [SerializeField] private Rigidbody _truckRigidbody;
    [SerializeField] private float _maxParkingSpeed = 0.5f;
    [SerializeField] private float _holdTime = 3f;
    [SerializeField] private Renderer _bayRenderer;

    public event Action<DeliveryBay> OnDeliveryComplete;
    public event Action<float> OnProgressUpdated;

    private Collider _bayCollider;
    private float _parkTimer;
    private bool _isTruckFullyInside;
    private bool _hasTriggered;

    private void Awake()
    {
        _bayCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (_hasTriggered) return;

        if (IsParkingValid())
        {
            _parkTimer += Time.deltaTime;
            OnProgressUpdated?.Invoke(_parkTimer / _holdTime);
            
            if (_parkTimer >= _holdTime)
            {
                _hasTriggered = true;
                OnProgressUpdated?.Invoke(0f);
                OnDeliveryComplete?.Invoke(this);
            }
        }
        else
        {
            if (_parkTimer > 0f)
            {
                _parkTimer = 0f;
                OnProgressUpdated?.Invoke(0f);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // This check ensures we only calculate the bounds of the main truck GameObject,
        // completely ignoring any child colliders like wheels or bumpers.
        if (other.gameObject == _truckRigidbody.gameObject)
        {
            bool containsMin = _bayCollider.bounds.Contains(other.bounds.min);
            bool containsMax = _bayCollider.bounds.Contains(other.bounds.max);
            _isTruckFullyInside = containsMin && containsMax;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _truckRigidbody.gameObject)
        {
            _isTruckFullyInside = false;
            _hasTriggered = false;
            _parkTimer = 0f;
            OnProgressUpdated?.Invoke(0f);
        }
    }

    private bool IsParkingValid()
    {
        if (!_isTruckFullyInside) return false;
        if (_truckRigidbody.linearVelocity.magnitude >= _maxParkingSpeed) return false;

        return true;
    }

    public void SetColor(Color color)
    {
        if (_bayRenderer != null)
        {
            _bayRenderer.material.color = color;
        }
    }
}