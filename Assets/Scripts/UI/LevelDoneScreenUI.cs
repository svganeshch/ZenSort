using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelDoneScreenUI : MonoBehaviour
{
    private UIDocument document;

    private Button continueButton;

    private void Awake()
    {
        document = GetComponent<UIDocument>();

        if (!document.enabled) document.enabled = true;

        continueButton = document.rootVisualElement.Q<Button>("ContinueButton");
        continueButton.RegisterCallback<ClickEvent>(ContinueButtonClicked);

        document.rootVisualElement.style.visibility = Visibility.Hidden;
    }

    private void ContinueButtonClicked(ClickEvent clickEvent)
    {
        StartCoroutine(GameManager.instance.LoadNextLevel());
        document.rootVisualElement.style.visibility = Visibility.Hidden;
    }
}
