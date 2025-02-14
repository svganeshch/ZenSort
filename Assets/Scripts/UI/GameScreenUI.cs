using UnityEngine;
using UnityEngine.UIElements;

public class GameScreenUI : MonoBehaviour
{
    private UIDocument document;

    private Label fpsCounter;

    private Button nextButton;

    private void Awake()
    {
        document = GetComponent<UIDocument>();

        fpsCounter = document.rootVisualElement.Q<Label>("fps_counter");

        nextButton = document.rootVisualElement.Q<Button>("NextButton");
        nextButton.RegisterCallback<ClickEvent>(OnNextButtonClick);
    }

    private void Update()
    {
        fpsCounter.text = FPSCounter.Instance.smoothFps.ToString("F2");
    }

    private void OnNextButtonClick(ClickEvent clickEvent)
    {
        StartCoroutine(GameManager.instance.LoadNextLevel());
    }
}
