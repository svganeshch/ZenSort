using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int currentLevel = 1;

    public int numberOfProps = 0;

    public ShelfManager shelfManager;

    public List<GameObject> shelfLayouts = new List<GameObject>();

    private List<Prop> levelProps = new List<Prop>();

    public void GenerateLevel()
    {
        GameManager.currentGameState = GameState.Active;

        GameObject levelShelfLayoutPrefab = shelfLayouts[Random.Range(0, shelfLayouts.Count)];
        GameObject levelShelfLayoutObj = Instantiate(levelShelfLayoutPrefab, transform);
        levelShelfLayoutObj.TryGetComponent<ShelfManager>(out shelfManager);

        numberOfProps = GetNumberOfProps(currentLevel);
        levelProps = GameManager.instance.propManager.GenerateProps(numberOfProps);
        shelfManager.StockShelfs(levelProps);
    }

    int GetNumberOfProps(int levelNumber)
    {
        int baseProps = 6;
        int scalingFactor = 3;

        int calculatedProps = baseProps + (levelNumber * scalingFactor);

        if (calculatedProps % 3 != 0)
        {
            calculatedProps += (3 - (calculatedProps % 3));
        }

        return calculatedProps;
    }

    public void GenerateNextLevel()
    {
        currentLevel++;
        GenerateLevel();
    }

    public IEnumerator ShuffleLevel()
    {
        yield return StartCoroutine(ClearLevel());

        GenerateLevel();
    }

    public IEnumerator ClearLevel()
    {
        DOTween.KillAll();

        bool shelfGridsCleared = shelfManager.ClearGrids();
        bool slotsCleared = GameManager.instance.slotManager.ClearAllSlots();

        Destroy(shelfManager.gameObject);
        levelProps.Clear();

        yield return new WaitUntil(() => shelfGridsCleared && slotsCleared);
    }
}
