using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int currentLevel = 1;

    public int numberOfProps = 0;

    public ShelfManager shelfManager;

    GameObject levelShelfLayoutPrefab;

    public void GenerateLevel()
    {
        GameManager.currentGameState = GameState.Active;

        LevelData existingLevelData = Resources.Load<LevelData>(currentLevel.ToString());
        if (existingLevelData != null)
        {
            levelShelfLayoutPrefab = existingLevelData.compartmentPrefab;
        }
        else
        {
            Debug.LogError($"Level data not found for level {currentLevel}!!");
        }

        GameObject levelShelfLayoutObj = Instantiate(levelShelfLayoutPrefab, transform);
        levelShelfLayoutObj.TryGetComponent<ShelfManager>(out shelfManager);

        numberOfProps = GetNumberOfProps(currentLevel);

        List<Prop> levelProps = new List<Prop>(GameManager.instance.propManager.GenerateProps(numberOfProps));

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

        UIManager.instance.OnLevelChange.Invoke(currentLevel);
    }
    
    public IEnumerator ReloadLevel()
    {
        yield return StartCoroutine(ClearLevel());

        GenerateLevel();
    }

    public IEnumerator LoadNextLevel()
    {
        yield return StartCoroutine(ClearLevel());

        GenerateNextLevel();
    }
    
    public IEnumerator ShuffleLevel()
    {
        yield return StartCoroutine(ClearProps());

        var existingPropsList = GameManager.instance.propManager.generatedProps;
        existingPropsList = existingPropsList.Where(item => item != null).ToList();
        
        shelfManager.StockShelfs(existingPropsList);
        //GenerateLevel(existingPropsList);
    }

    public IEnumerator ClearProps()
    {
        DOTween.KillAll(complete: true);

        bool shelfGridsCleared = shelfManager.ClearGrids(true);
        
        yield return new WaitUntil(() => shelfGridsCleared);
    }

    public IEnumerator ClearLevel()
    {
        DOTween.KillAll(complete: true);

        bool shelfGridsCleared = shelfManager.ClearGrids();
        bool slotsCleared = GameManager.instance.slotManager.ClearAllSlots();

        Destroy(shelfManager.gameObject);

        // Reset UI
        UIManager.instance.progressBar.ResetProgressBar();
        UIManager.instance.starCountHandler.ResetStarCount();
        UIManager.instance.bonusStarHandler.RemoveBonusStars();

        yield return new WaitUntil(() => shelfGridsCleared && slotsCleared);
    }
}
