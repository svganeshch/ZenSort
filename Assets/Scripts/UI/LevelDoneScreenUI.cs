using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelDoneScreenUI : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public Button continueButton;

    private void Awake()
    {
        continueButton.onClick.AddListener(() => StartCoroutine(ContinueButtonClicked()));
    }

    public void ShowPopUp()
    {
        gameObject.SetActive(true);

        levelText.text = $"LEVEL {GameManager.instance.levelManager.currentLevel}";
        transform.DOPunchScale(Vector3.one * 0.25f, 0.5f, 0, 0);
    }

    public IEnumerator ContinueButtonClicked()
    {
        yield return StartCoroutine(GameManager.instance.LoadNextLevel());

        UIManager.instance.gameScreenUI.rootVisualElement.style.visibility = UnityEngine.UIElements.Visibility.Visible;
        UIManager.instance.progressBar.ResetProgressBar();
        
        gameObject.SetActive(false);
    }
}
