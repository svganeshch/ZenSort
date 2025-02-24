using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image progressBar;
    public TextMeshProUGUI progressText;

    public void SetProgress(float progress)
    {
        progressBar.DOFillAmount(progress / 100, 0.5f);
        progressText.text = $"{(int)progress}%";
    }

    public void ResetProgressBar()
    {
        progressBar.DOFillAmount(0, 0.5f);
        progressText.text = $"{0}%";
    }
}
