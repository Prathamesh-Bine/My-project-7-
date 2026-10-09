using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ParkingDetector : MonoBehaviour
{
    [SerializeField] private Rigidbody _truckRigidbody;
    [SerializeField] private float _maxParkingAngle = 15f;
    [SerializeField] private float _maxParkingSpeed = 0.5f;
    [SerializeField] private float _holdTime = 3f;
    [SerializeField] private Renderer _bayRenderer;

    public event Action<ParkingDetector> OnParked;

    private Collider _bayCollider;
    private float _parkTimer;
    private bool _isTruckFullyInside;
    private bool _hasTriggeredPark;

    private void Awake()
    {
        _bayCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (_hasTriggeredPark) return;

        if (IsParkingValid())
        {
            _parkTimer += Time.deltaTime;
            
            if (_parkTimer >= _holdTime)
            {
                _hasTriggeredPark = true;
                OnParked?.Invoke(this);
            }
        }
        else
        {
            _parkTimer = 0f;
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
            _hasTriggeredPark = false;
            _parkTimer = 0f;
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

    public void SetColor(Color color)
    {
        if (_bayRenderer != null)
        {
            _bayRenderer.material.color = color;
        }
    }
}