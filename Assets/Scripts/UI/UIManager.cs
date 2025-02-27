using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public UIDocument levelDoneScreen;
    public UIDocument gameOverScreen;

    public UnityEvent OnLevelDone;
    public UnityEvent OnGameOver;

    public UnityEvent<int> OnLevelChange;

    public ProgressBar progressBar;
    public ComboTextHandler comboTextHandler;
    public StarCountHandler starCountHandler;
    public BonusStarHandler bonusStarHandler;

    private void Awake()
    {
        instance = this;

        progressBar = GetComponentInChildren<ProgressBar>();
        comboTextHandler = GetComponentInChildren<ComboTextHandler>();
        starCountHandler = GetComponentInChildren<StarCountHandler>();
        bonusStarHandler = GetComponentInChildren<BonusStarHandler>();

        OnLevelDone = new UnityEvent();
        OnGameOver = new UnityEvent();

        OnLevelChange = new UnityEvent<int>();

        OnLevelDone.AddListener(ShowLevelDoneScreen);
        OnGameOver.AddListener(ShowGameOverScreen);
    }

    private void ShowLevelDoneScreen()
    {
        GameManager.currentGameState = GameState.Paused;
        levelDoneScreen.rootVisualElement.style.visibility = Visibility.Visible;
    }

    private void ShowGameOverScreen()
    {
        GameManager.currentGameState = GameState.Paused;
        gameOverScreen.rootVisualElement.style.visibility = Visibility.Visible;
    }
}
