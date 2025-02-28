using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public UIDocument gameScreenUI;

    public UnityEvent OnLevelDone;
    public UnityEvent OnGameOver;

    public UnityEvent<int> OnLevelChange;

    public ProgressBar progressBar;
    public ComboTextHandler comboTextHandler;
    public StarCountHandler starCountHandler;
    public BonusStarHandler bonusStarHandler;

    public LevelDoneScreenUI levelDoneScreen;
    public GameOverScreenUI gameOverScreen;

    private void Awake()
    {
        instance = this;

        progressBar = GetComponentInChildren<ProgressBar>();
        comboTextHandler = GetComponentInChildren<ComboTextHandler>();
        starCountHandler = GetComponentInChildren<StarCountHandler>();
        bonusStarHandler = GetComponentInChildren<BonusStarHandler>();

        levelDoneScreen = GetComponentInChildren<LevelDoneScreenUI>(includeInactive: true);
        gameOverScreen = GetComponentInChildren<GameOverScreenUI>(includeInactive: true);

        OnLevelDone = new UnityEvent();
        OnGameOver = new UnityEvent();

        OnLevelChange = new UnityEvent<int>();

        OnLevelDone.AddListener(ShowLevelDoneScreen);
        OnGameOver.AddListener(ShowGameOverScreen);
    }

    private void ShowLevelDoneScreen()
    {
        GameManager.currentGameState = GameState.Paused;

        gameScreenUI.rootVisualElement.style.visibility = Visibility.Hidden;
        levelDoneScreen.ShowPopUp();

        SFXManager.instance.PlaySFX(SFXManager.instance.levelDoneSound, volume: 2);
    }

    private void ShowGameOverScreen()
    {
        GameManager.currentGameState = GameState.Paused;

        gameScreenUI.rootVisualElement.style.visibility = Visibility.Hidden;
        gameOverScreen.ShowPopUP();

        SFXManager.instance.PlaySFX(SFXManager.instance.levelFailedSound, volume: 2);
    }
}
