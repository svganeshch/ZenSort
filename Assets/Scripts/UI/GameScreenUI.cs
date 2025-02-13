using UnityEngine;
using UnityEngine.UIElements;

public class GameScreenUI : MonoBehaviour
{
    private UIDocument document;

    private Label fpsCounter;

    private void Awake()
    {
        document = GetComponent<UIDocument>();

        fpsCounter = document.rootVisualElement.Q<Label>("fps_counter");
    }

    private void Update()
    {
        fpsCounter.text = FPSCounter.Instance.smoothFps.ToString("F2");
    }
}
