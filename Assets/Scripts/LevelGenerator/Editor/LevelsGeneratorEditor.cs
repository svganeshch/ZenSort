using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelsGeneratorTool))]
public class LevelsGeneratorEditor : Editor
{
    public string levelsPath = $"Assets/Data/Levels/Resources/";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelsGeneratorTool tool = (LevelsGeneratorTool)target;

        if (GUILayout.Button("Generate"))
        {
            GenerateLevelData(tool);
        }

        //if (GUILayout.Button("Load Level"))
        //{
        //    tool.LoadLevelData();
        //}

        //if (GUILayout.Button("Clear"))
        //{
        //    tool.ClearLevel();
        //}

        if (GUILayout.Button("Save"))
        {
            SaveLevelAsset(tool);
        }
    }

    private void GenerateLevelData(LevelsGeneratorTool levelsGeneratorTool)
    {
        //if (levelsGeneratorTool.loadLevelData != null)
        //{
        //    var currentLevelData = GenerateRandomBaseLevel(levelsGeneratorTool);
        //    SaveLevelAsset(levelsGeneratorTool, currentLevelData);

        //    levelsGeneratorTool.loadLevelData = currentLevelData;

        //    return;
        //}

        for (int i = 0; i < levelsGeneratorTool.numberOfLevelsToProduce; i++)
        {
            var currentLevelData = GenerateRandomBaseLevel(levelsGeneratorTool);
            SaveLevelAsset(levelsGeneratorTool, currentLevelData);

            levelsGeneratorTool.currentLevelNum++;
        }
    }

    private void SaveLevelAsset(LevelsGeneratorTool levelsGeneratorTool, LevelData levelData = null)
    {
        AssetDatabase.CreateAsset(levelData, $"{levelsPath}{levelsGeneratorTool.currentLevelNum}.asset");
    }

    public LevelData GenerateRandomBaseLevel(LevelsGeneratorTool levelsGeneratorTool)
    {
        LevelData randomLevel = ScriptableObject.CreateInstance<LevelData>();

        randomLevel.compartmentPrefab = levelsGeneratorTool.compartmentPrefabs[Random.Range(0, levelsGeneratorTool.compartmentPrefabs.Length)];

        return randomLevel;
    }
}