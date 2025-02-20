using UnityEngine;
using UnityEngine.UIElements;

public class GameOverScreenUI : MonoBehaviour
{
    private UIDocument document;

    private Button replayButton;

    private void Awake()
    {
        document = GetComponent<UIDocument>();

        if (!document.enabled) document.enabled = true;

        replayButton = document.rootVisualElement.Q<Button>("ReplayButton");
        replayButton.RegisterCallback<ClickEvent>(ReplayButtonClicked);

        document.rootVisualElement.style.visibility = Visibility.Hidden;
    }

    private void ReplayButtonClicked(ClickEvent clickEvent)
    {
        StartCoroutine(GameManager.instance.ReloadLevel());
        document.rootVisualElement.style.visibility = Visibility.Hidden;
    }
}
