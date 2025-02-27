using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarCountHandler : MonoBehaviour
{
    public Image starImage;
    public TextMeshProUGUI starCountText;

    public void AddStar()
    {
        starCountText.text = (int.Parse(starCountText.text) + 1).ToString();
        PlayStarAddTween();
    }

    public void PlayStarAddTween()
    {
        starImage.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 0, 0).SetEase(Ease.InQuad);
    }
}
