using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [HideInInspector] public LevelManager levelManager;
    [HideInInspector] public PropManager propManager;
    [HideInInspector] public ShelfManager shelfManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        levelManager = GetComponentInChildren<LevelManager>();
        propManager = GetComponentInChildren<PropManager>();

        shelfManager = FindFirstObjectByType<ShelfManager>();
    }
}
