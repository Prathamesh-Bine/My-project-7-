using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeliveryUIManager : MonoBehaviour
{
    [SerializeField] private DeliveryManager _deliveryManager;
    [SerializeField] private TextMeshProUGUI _objectiveLabel;
    [SerializeField] private TextMeshProUGUI _countdownLabel;
    [SerializeField] private TextMeshProUGUI _deliveriesLabel;
    [SerializeField] private Image _progressBar;
    
    [Header("Time Up Panel")]
    [SerializeField] private GameObject _timeUpPanel;
    [SerializeField] private TextMeshProUGUI _timeUpText;
    [SerializeField] private Button _restartButton;
    
    [Header("Colors")]
    [SerializeField] private Color _normalTimeColor = Color.white;
    [SerializeField] private Color _warningTimeColor = Color.red;

    private Coroutine _deliveredMessageCoroutine;

    private void OnEnable()
    {
        if (_deliveryManager != null)
        {
            _deliveryManager.OnCrateLoaded += HandleCrateLoaded;
            _deliveryManager.OnDelivered += HandleDelivered;
            _deliveryManager.OnTimeUpdated += HandleTimeUpdated;
            _deliveryManager.OnParkProgressUpdated += HandleProgressUpdated;
            _deliveryManager.OnTimeUp += HandleTimeUp;
            _deliveryManager.OnGameReset += HandleGameReset;
            
            _restartButton.onClick.AddListener(_deliveryManager.RestartGame);
        }
    }

    private void OnDisable()
    {
        if (_deliveryManager != null)
        {
            _deliveryManager.OnCrateLoaded -= HandleCrateLoaded;
            _deliveryManager.OnDelivered -= HandleDelivered;
            _deliveryManager.OnTimeUpdated -= HandleTimeUpdated;
            _deliveryManager.OnParkProgressUpdated -= HandleProgressUpdated;
            _deliveryManager.OnTimeUp -= HandleTimeUp;
            _deliveryManager.OnGameReset -= HandleGameReset;
            
            _restartButton.onClick.RemoveListener(_deliveryManager.RestartGame);
        }
    }

    private void Start()
    {
        HandleGameReset();
    }

    private void HandleProgressUpdated(float progressFill)
    {
        if (_progressBar != null)
        {
            _progressBar.fillAmount = progressFill;
        }
    }

    private void HandleCrateLoaded(string targetBayName)
    {
        if (_deliveredMessageCoroutine != null) StopCoroutine(_deliveredMessageCoroutine);
        
        _objectiveLabel.text = $"Deliver to {targetBayName}";
        _countdownLabel.gameObject.SetActive(true);
        _countdownLabel.color = _normalTimeColor;
    }

    private void HandleDelivered(int totalDeliveries)
    {
        _countdownLabel.gameObject.SetActive(false);
        _deliveriesLabel.text = $"Deliveries: {totalDeliveries}";
        
        if (_deliveredMessageCoroutine != null) StopCoroutine(_deliveredMessageCoroutine);
        _deliveredMessageCoroutine = StartCoroutine(ShowDeliveredRoutine());
    }

    private void HandleTimeUpdated(int timeLeft)
    {
        _countdownLabel.text = $"Time left: {timeLeft} s";
        
        if (timeLeft <= 10)
        {
            _countdownLabel.color = _warningTimeColor;
        }
    }

    private void HandleTimeUp(int totalDeliveries)
    {
        _timeUpPanel.SetActive(true);
        _timeUpText.text = $"Time up! Deliveries: {totalDeliveries}";
    }

    private void HandleGameReset()
    {
        if (_deliveredMessageCoroutine != null) StopCoroutine(_deliveredMessageCoroutine);
        
        _timeUpPanel.SetActive(false);
        _objectiveLabel.text = "Go to the loading bay";
        _countdownLabel.gameObject.SetActive(false);
        _deliveriesLabel.text = "Deliveries: 0";
        if (_progressBar != null) _progressBar.fillAmount = 0f;
    }

    private IEnumerator ShowDeliveredRoutine()
    {
        _objectiveLabel.text = "Delivered! +1";
        yield return new WaitForSeconds(2f);
        _objectiveLabel.text = "Go to the loading bay";
    }
}