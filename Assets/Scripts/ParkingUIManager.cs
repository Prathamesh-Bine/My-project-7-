using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ParkingUIManager : MonoBehaviour
{
    [SerializeField] private ParkingBay _parkingBay;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private Image _progressBar;

    private int _lastDisplayedTime = -1;

    private void OnEnable()
    {
        if (_parkingBay != null)
        {
            _parkingBay.OnTimerUpdated += HandleTimerUpdated;
            _parkingBay.OnProgressUpdated += HandleProgressUpdated;
            _parkingBay.OnParked += HandleParked;
            _parkingBay.OnGameReset += HandleGameReset;
        }
        else
        {
            Debug.LogError($"No ParkingBay assigned to {name}", this);
        }
    }

    private void OnDisable()
    {
        if (_parkingBay != null)
        {
            _parkingBay.OnTimerUpdated -= HandleTimerUpdated;
            _parkingBay.OnProgressUpdated -= HandleProgressUpdated;
            _parkingBay.OnParked -= HandleParked;
            _parkingBay.OnGameReset -= HandleGameReset;
        }
    }

    private void HandleTimerUpdated(float elapsedTime)
    {
        int currentSeconds = Mathf.FloorToInt(elapsedTime);
        
        if (currentSeconds != _lastDisplayedTime)
        {
            _lastDisplayedTime = currentSeconds;
            _timerText.text = $"Time: {currentSeconds} s";
        }
    }

    private void HandleProgressUpdated(float progressFill)
    {
        _progressBar.fillAmount = progressFill;
    }

    private void HandleParked(float finalTime)
    {
        _resultText.text = $"Parked! Time: {Mathf.FloorToInt(finalTime)} s";
    }

    private void HandleGameReset()
    {
        _lastDisplayedTime = -1;
        _timerText.text = "Time: 0 s";
        _progressBar.fillAmount = 0f;
        _resultText.text = string.Empty;
    }
}