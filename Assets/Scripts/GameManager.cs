using DG.Tweening.Core.Easing;
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
    [HideInInspector] public SaveManager saveManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        levelManager = GetComponentInChildren<LevelManager>();
        propManager = GetComponentInChildren<PropManager>();

        slotManager = FindFirstObjectByType<SlotManager>();
        saveManager = GetComponentInChildren<SaveManager>();
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        levelManager.currentLevel = saveManager.saveData.currentLevel;
        UIManager.instance.OnLevelChange.Invoke(levelManager.currentLevel);

        levelManager.GenerateLevel();
    }
}
