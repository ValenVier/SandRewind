using UnityEngine;
using UnityEngine.UI;

/// <summary>Fills/drains a UI Image based on PlayerDeathHandler's grace window timer</summary>
public class HealthBarHUD : MonoBehaviour
{
    [SerializeField] private PlayerDeathHandler deathHandler;
    [SerializeField] private Image fillImage;

    private void OnEnable()
    {
        if (deathHandler != null)
        {
            deathHandler.OnGraceTimeChanged.AddListener(UpdateFill);
        }
    }

    private void OnDisable()
    {
        if (deathHandler != null)
        {
            deathHandler.OnGraceTimeChanged.RemoveListener(UpdateFill);
        }
    }

    private void UpdateFill(float remaining, float total)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = total > 0f ? remaining / total : 0f;
    }
}