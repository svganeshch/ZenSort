using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarCountHandler : MonoBehaviour
{
    public Image starImage;
    public TextMeshProUGUI starCountText;

    private Tween starAddTween;

    private void Start()
    {
        UIManager.instance.OnLevelChange.AddListener(ResetStarCount);
    }

    public void AddStar(int count = 1)
    {
        starCountText.text = (int.Parse(starCountText.text) + count).ToString();

        PlayStarAddTween();
    }

    public void PlayStarAddTween()
    {
        if (starAddTween != null && starAddTween.IsActive()) starAddTween.Complete();
        starAddTween = starImage.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 0, 0).SetEase(Ease.InQuad);
    }

    public void ResetStarCount(int level = 0)
    {
        starCountText.text = "0";
    }
}
