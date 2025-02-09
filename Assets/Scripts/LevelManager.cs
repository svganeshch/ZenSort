using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int currentLevel = 1;

    public int numberOfProps = 0;
    private List<Prop> levelProps = new List<Prop>();

    private void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        GameManager.currentGameState = GameState.Active;

        numberOfProps = GetNumberOfProps(currentLevel);
        levelProps = GameManager.instance.propManager.GenerateProps(numberOfProps);
        GameManager.instance.shelfManager.StockShelfs(levelProps);
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
}
