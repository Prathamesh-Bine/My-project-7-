using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeliveryManager : MonoBehaviour
{
    [SerializeField] private LoadingBay _loadingBay;
    [SerializeField] private DeliveryBay[] _deliveryBays;
    [SerializeField] private GameObject _cratePrefab;
    [SerializeField] private Transform _truckTransform;
    [SerializeField] private Vector3 _crateLocalOffset = new Vector3(0f, 1f, -1f);
    
    [SerializeField] private TruckMovement _truckMovement;
    [SerializeField] private Rigidbody _truckRigidbody;
    
    [SerializeField] private int _countdownStartTime = 60;

    public event Action<string> OnCrateLoaded;
    public event Action<int> OnDelivered;
    public event Action<int> OnTimeUpdated;
    public event Action<float> OnParkProgressUpdated;
    public event Action<int> OnTimeUp;
    public event Action OnGameReset;

    private GameObject _activeCrate;
    private DeliveryBay _targetBay;
    private Coroutine _countdownCoroutine;
    private int _totalDeliveries = 0;
    private TruckInput _truckInput;

    private void Awake()
    {
        _truckInput = new TruckInput();
        _truckInput.Move.Restart.performed += ctx => RestartGame();
    }

    private void OnEnable()
    {
        _truckInput.Enable();
        _loadingBay.OnLoadComplete += HandleLoadComplete;
        _loadingBay.OnProgressUpdated += HandleProgressUpdated;

        foreach (DeliveryBay bay in _deliveryBays)
        {
            bay.OnDeliveryComplete += HandleDeliveryComplete;
            bay.OnProgressUpdated += HandleProgressUpdated;
            bay.SetColor(Color.grey);
        }
    }

    private void OnDisable()
    {
        _truckInput.Disable();
        _loadingBay.OnLoadComplete -= HandleLoadComplete;
        _loadingBay.OnProgressUpdated -= HandleProgressUpdated;

        foreach (DeliveryBay bay in _deliveryBays)
        {
            bay.OnDeliveryComplete -= HandleDeliveryComplete;
            bay.OnProgressUpdated -= HandleProgressUpdated;
        }
    }

    private void HandleProgressUpdated(float progress)
    {
        OnParkProgressUpdated?.Invoke(progress);
    }

    private void HandleLoadComplete()
    {
        if (_activeCrate == null)
        {
            LoadCrate();
        }
    }

    private void HandleDeliveryComplete(DeliveryBay bay)
    {
        if (bay == _targetBay && _activeCrate != null)
        {
            DeliverCrate();
        }
    }

    private void LoadCrate()
    {
        _activeCrate = Instantiate(_cratePrefab, _truckTransform);
        _activeCrate.transform.localPosition = _crateLocalOffset;

        int randomIndex = UnityEngine.Random.Range(0, _deliveryBays.Length);
        _targetBay = _deliveryBays[randomIndex];
        _targetBay.SetColor(Color.green);

        OnCrateLoaded?.Invoke(_targetBay.gameObject.name);

        if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
        _countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    private void DeliverCrate()
    {
        Destroy(_activeCrate);
        _activeCrate = null;
        
        _targetBay.SetColor(Color.grey);
        _targetBay = null;

        if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
        
        _totalDeliveries++;
        OnDelivered?.Invoke(_totalDeliveries);
    }

    private IEnumerator CountdownRoutine()
    {
        int timeLeft = _countdownStartTime;
        
        while (timeLeft > 0)
        {
            OnTimeUpdated?.Invoke(timeLeft);
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        OnTimeUpdated?.Invoke(0);
        TriggerTimeUp();
    }

    private void TriggerTimeUp()
    {
        _truckMovement.enabled = false;
        _truckRigidbody.linearVelocity = Vector3.zero;
        _truckRigidbody.angularVelocity = Vector3.zero;
        
        OnTimeUp?.Invoke(_totalDeliveries);
    }

    public void RestartGame()
    {
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }

        if (_activeCrate != null)
        {
            Destroy(_activeCrate);
            _activeCrate = null;
        }

        if (_targetBay != null)
        {
            _targetBay.SetColor(Color.grey);
            _targetBay = null;
        }

        _totalDeliveries = 0;
        
        _truckMovement.enabled = true;
        _truckMovement.ResetPosition();

        OnGameReset?.Invoke();
    }
}