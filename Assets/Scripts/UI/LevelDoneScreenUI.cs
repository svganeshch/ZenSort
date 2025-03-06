using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelDoneScreenUI : MonoBehaviour
{
    public GameObject levelDonePopUpPanel;
    public GameObject levelDoneCeleb;
    
    public TextMeshProUGUI levelText;
    public Button continueButton;

    private void Awake()
    {
        continueButton.onClick.AddListener(() => StartCoroutine(ContinueButtonClicked()));
    }

    public IEnumerator ShowPopUp()
    {
        gameObject.SetActive(true);
        levelDoneCeleb.gameObject.SetActive(true);

        yield return new WaitForSeconds(4);
        
        levelDoneCeleb.gameObject.SetActive(false);
        
        levelDonePopUpPanel.SetActive(true);
        levelText.text = $"LEVEL {GameManager.instance.levelManager.currentLevel}";
        transform.DOPunchScale(Vector3.one * 0.25f, 0.5f, 0, 0);
    }

    public IEnumerator ContinueButtonClicked()
    {
        SFXManager.instance.PlaySFX(SFXManager.instance.buttonTap);
        
        //SFXManager.instance.QuickFade();

        yield return StartCoroutine(GameManager.instance.levelManager.LoadNextLevel());

        UIManager.instance.gameScreenUI.rootVisualElement.style.visibility = UnityEngine.UIElements.Visibility.Visible;

        levelDonePopUpPanel.transform.DOScale(Vector3.one * 0.25f, 0.25f)
            .OnComplete(() =>
            {
                levelDonePopUpPanel.SetActive(false);
                gameObject.SetActive(false);
                
                levelDonePopUpPanel.transform.localScale = Vector3.one;
            });
    }
}
