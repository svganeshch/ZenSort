using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreenUI : MonoBehaviour
{
    public Image dropDownBanner;

    public GameObject gameOverPopUp;
    public TextMeshProUGUI levelText;
    public Button replayButton;

    private void Awake()
    {
        replayButton.onClick.AddListener(() => ReplayButtonClicked());
    }

    public void ShowPopUP()
    {
        gameObject.SetActive(true);

        PlayDropDownAnim();
    }

    private void PlayDropDownAnim()
    {
        dropDownBanner.gameObject.SetActive(true);
        Vector3 bannerOrigPos = dropDownBanner.transform.position;

        Sequence gameOverSeq = DOTween.Sequence();

        gameOverSeq.Append(dropDownBanner.transform.DOLocalMoveY(5, 1f).SetEase(Ease.OutBounce))
           .AppendInterval(0.2f)
           .Append(dropDownBanner.transform.DOLocalMoveY(-1500, 0.8f).SetEase(Ease.InQuad))
           .OnComplete(() =>
           {
               dropDownBanner.transform.position = bannerOrigPos;
               ShowGameOverContainer();
           });
    }

    private void ShowGameOverContainer()
    {
        gameOverPopUp.SetActive(true);
        levelText.text = $"LEVEL {GameManager.instance.levelManager.currentLevel}";
        gameOverPopUp.transform.DOPunchScale(Vector3.one * 0.25f, 0.5f, 0, 0);
    }

    private void ReplayButtonClicked()
    {
        StartCoroutine(GameManager.instance.levelManager.ReloadLevel());

        UIManager.instance.gameScreenUI.rootVisualElement.style.visibility = UnityEngine.UIElements.Visibility.Visible;

        gameOverPopUp.SetActive(false);
    }
}
