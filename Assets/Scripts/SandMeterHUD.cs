using UnityEngine;
using UnityEngine.UI;

/// <summary>Fills/drains a UI Image based on SandMeter's current value</summary>
public class SandMeterHUD : MonoBehaviour
{
    [SerializeField] private SandMeter sandMeter;
    [SerializeField] private Image fillImage;

    private void OnEnable()
    {
        if (sandMeter != null)
        {
            sandMeter.OnSandChanged.AddListener(UpdateFill);
            UpdateFill(sandMeter.CurrentSand, sandMeter.MaxSand); // show starting value immediately
        }
    }

    private void OnDisable()
    {
        if (sandMeter != null)
        {
            sandMeter.OnSandChanged.RemoveListener(UpdateFill);
        }
    }

    private void UpdateFill(float current, float max)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = max > 0f ? current / max : 0f;
    }
}