using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GridData
{
    public Vector3 position;
    public Vector3 scale;
}

[System.Serializable]
public class ObstacleData
{
    public string name;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}
[System.Serializable]
public class RouteData
{
    public int id;
    public StartPointData startPoint;
    public EndPointData endPoint;
}

[System.Serializable]
public class StartPointData
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}

[System.Serializable]
public class EndPointData
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}


[System.Serializable]
public class LevelData
{
    public int levelNumber;
    public List<GridData> grids;
    public List<ObstacleData> obstacles;
    public List<StartPointData> startPoints;
    public List<EndPointData> endPoints;
    public List<RouteData> routes;
}


public static class LevelManager
{
    private static string levelsFolderPath = $"{Application.dataPath}/Levels";
    public static void SaveLevel(LevelData levelData, string fileName)
    {
        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText($"{levelsFolderPath}/{fileName}.json", json);
    }

    public static LevelData LoadLevel(string fileName)
    {
        string filePath = $"{levelsFolderPath}/{fileName}.json";
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<LevelData>(json);
        }
        return null;
    }
}
