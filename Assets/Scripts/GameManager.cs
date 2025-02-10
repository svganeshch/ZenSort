using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static GameState currentGameState;

    [HideInInspector] public LevelManager levelManager;
    [HideInInspector] public PropManager propManager;
    [HideInInspector] public ShelfManager shelfManager;
    [HideInInspector] public SlotManager slotManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        levelManager = GetComponentInChildren<LevelManager>();
        propManager = GetComponentInChildren<PropManager>();

        shelfManager = FindFirstObjectByType<ShelfManager>();
        slotManager = FindFirstObjectByType<SlotManager>();
    }
}
