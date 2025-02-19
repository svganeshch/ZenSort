using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static GameState currentGameState;

    [HideInInspector] public LevelManager levelManager;
    [HideInInspector] public PropManager propManager;
    [HideInInspector] public SlotManager slotManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        levelManager = GetComponentInChildren<LevelManager>();
        propManager = GetComponentInChildren<PropManager>();

        slotManager = FindFirstObjectByType<SlotManager>();
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        levelManager.GenerateLevel();
    }

    public IEnumerator ReloadLevel()
    {
        yield return StartCoroutine(levelManager.ClearLevel());

        levelManager.GenerateLevel();
    }

    public IEnumerator LoadNextLevel()
    {
        yield return StartCoroutine(levelManager.ClearLevel());

        levelManager.GenerateNextLevel();
    }
}
