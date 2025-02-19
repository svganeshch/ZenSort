using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public UIDocument levelDoneScreen;

    public UnityEvent OnLevelDone;

    private void Awake()
    {
        instance = this;

        OnLevelDone = new UnityEvent();

        OnLevelDone.AddListener(ShowLevelDoneScreen);
    }

    private void ShowLevelDoneScreen()
    {
        GameManager.currentGameState = GameState.Paused;
        levelDoneScreen.rootVisualElement.style.visibility = Visibility.Visible;
    }
}
