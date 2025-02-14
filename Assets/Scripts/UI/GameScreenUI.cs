using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameScreenUI : MonoBehaviour
{
    private UIDocument document;

    private Label fpsCounter;

    private Button nextButton;

    private Button undoButton;
    private Button magnetButton;
    private Button shuffleButton;

    private void Awake()
    {
        document = GetComponent<UIDocument>();

        fpsCounter = document.rootVisualElement.Q<Label>("fps_counter");

        nextButton = document.rootVisualElement.Q<Button>("NextButton");
        nextButton.RegisterCallback<ClickEvent>(OnNextButtonClick);

        undoButton = document.rootVisualElement.Q<Button>("UndoButton");
        undoButton.RegisterCallback<ClickEvent>(OnUndoButtonClick);

        magnetButton = document.rootVisualElement.Q<Button>("MagnetButton");
        magnetButton.RegisterCallback<ClickEvent>(OnMagnetButtonClick);

        shuffleButton = document.rootVisualElement.Q<Button>("ShuffleButton");
        shuffleButton.RegisterCallback<ClickEvent>(OnShuffleButtonClick);
    }


    private void Update()
    {
        fpsCounter.text = FPSCounter.Instance.smoothFps.ToString("F2");
    }

    private void OnNextButtonClick(ClickEvent clickEvent)
    {
        StartCoroutine(GameManager.instance.LoadNextLevel());
    }

    private void OnUndoButtonClick(ClickEvent clickEvent)
    {
        BoosterManager.instance.HandleUndoBooster();
    }
    private void OnMagnetButtonClick(ClickEvent evt)
    {
        BoosterManager.instance.HandleMagnetBooster();
    }
    private void OnShuffleButtonClick(ClickEvent evt)
    {
        BoosterManager.instance.HandleShuffleBooster();
    }
}
