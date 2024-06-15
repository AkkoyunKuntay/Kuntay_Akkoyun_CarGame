using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorScripts : MonoBehaviour
{
}

public class LevelEditorWindow : EditorWindow
{
    [Header("Grid Configurations")]
    private GameObject groundPrefab;
    private Vector2Int gridSize;
    private GameObject lastCreatedGrid;
    private bool snapToGrid;
    private float gridSizeFloat = 1.0f;

    [Header("Obstacle Configurations")]
    private ObstacleListSO obstacleList;
    private GameObject selectedObstacle;
    private bool isPlacing;
    private bool isDeleting; // Silme bayraðý
    private float rotationAngle;
    private float rotation;
    private GameObject lastPlacedObstacle;

    private GameObject container;  // Container for all grid and obstacle objects

    private Route routePrefab;
    private GameObject startPointPrefab;
    private GameObject endPointPrefab;
    private List<Route> routes = new List<Route>();
    private int currentId = 0;
    private int levelNumber;

    [MenuItem("Window/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Layout", EditorStyles.boldLabel);
        gridSize = EditorGUILayout.Vector2IntField("GridSize", gridSize);
        groundPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", groundPrefab, typeof(GameObject), false);
        container = (GameObject)EditorGUILayout.ObjectField("Container", container, typeof(GameObject), true);

        if (GUILayout.Button("Generate Grid"))
        {
            DestroyImmediate(lastCreatedGrid);
            lastCreatedGrid = Instantiate(groundPrefab, new Vector3(gridSize.x / 2f, 0, gridSize.y / 2f), Quaternion.identity);
            lastCreatedGrid.transform.localScale = new Vector3(gridSize.x, 0.1f, gridSize.y);

            if (container != null)
            {
                lastCreatedGrid.transform.SetParent(container.transform);
            }
        }

        GUILayout.Space(10);

        GUILayout.Label("Obstacle List", EditorStyles.boldLabel);
        obstacleList = (ObstacleListSO)EditorGUILayout.ObjectField("Obstacle List", obstacleList, typeof(ObstacleListSO), false);

        if (obstacleList != null)
        {
            for (int i = 0; i < obstacleList.obstacles.Length; i++)
            {
                if (GUILayout.Button($"Select {obstacleList.obstacles[i].name}"))
                {
                    selectedObstacle = obstacleList.obstacles[i];
                    isPlacing = true;
                    rotationAngle = 0f;
                }
            }
        }

        if (isPlacing && selectedObstacle != null)
        {
            GUILayout.Label($"Placing: {selectedObstacle.name}", EditorStyles.boldLabel);
            rotationAngle = EditorGUILayout.FloatField("Rotation Angle", rotationAngle);
            snapToGrid = EditorGUILayout.Toggle("Snap to Grid", snapToGrid);
            if (snapToGrid)
            {
                gridSizeFloat = EditorGUILayout.FloatField("Grid Size", gridSizeFloat);
            }
            if (GUILayout.Button("Rotate " + rotation + "°"))
            {
                rotation = 45;
                rotationAngle += rotation;
                if (rotationAngle >= 360f)
                {
                    rotationAngle -= 360f;
                }
            }
            if (GUILayout.Button("Remove Last"))
            {
                if (lastPlacedObstacle != null)
                {
                    DestroyImmediate(lastPlacedObstacle);
                }
            }
            if (GUILayout.Button("Cancel"))
            {
                selectedObstacle = null;
                isPlacing = false;
            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Delete Mode"))
        {
            isPlacing = false; // Yerleþtirme modunu devre dýþý býrak
            isDeleting = true;
        }

        GUILayout.Space(10);

        routePrefab = (Route)EditorGUILayout.ObjectField("RoutePrefab", routePrefab, typeof(Route), false);
        GUILayout.Label("Start/End Points", EditorStyles.boldLabel);
        startPointPrefab = (GameObject)EditorGUILayout.ObjectField("Start Point Prefab", startPointPrefab, typeof(GameObject), false);
        endPointPrefab = (GameObject)EditorGUILayout.ObjectField("End Point Prefab", endPointPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Place Route"))
        {
            if (startPointPrefab != null && endPointPrefab != null)
            {
                PlaceRoute();
            }
        }

        GUILayout.Space(10);

        GUILayout.Label("Routes", EditorStyles.boldLabel);
        for (int i = 0; i < routes.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Route {routes[i].name}");
            if (GUILayout.Button("Remove"))
            {
                RemoveRoute(i);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        GUILayout.Label("Level Number", EditorStyles.boldLabel);
        levelNumber = EditorGUILayout.IntField("Level Number", levelNumber);

        if (GUILayout.Button("Save Level"))
        {
            SaveCurrentLevel();
        }

        if (GUILayout.Button("Load Level"))
        {
            LoadLevel($"Level{levelNumber}");  // Örnek dosya adý, deðiþtirebilirsiniz
        }

        if (GUILayout.Button("Clear All"))
        {
            // Tüm route'larý temizle
            for (int i = routes.Count - 1; i >= 0; i--)
            {
                RemoveRoute(i);
            }

            // Container altýndaki tüm çocuklarý temizle
            for (int i = container.transform.childCount - 1; i >= 0; --i)
            {
                DestroyImmediate(container.transform.GetChild(i).gameObject);
            }

            // Grid ve son yerleþtirilen engelleri temizle
            DestroyImmediate(lastCreatedGrid);
            lastPlacedObstacle = null;
            lastCreatedGrid = null;

            // Seçim ve bayraklarý sýfýrla
            selectedObstacle = null;
            isPlacing = false;
            isDeleting = false;

            // Route ID'yi sýfýrla
            //currentId = 0;
        }
    }

    private void PlaceRoute()
    {
        Vector3 startPosition = Vector3.zero;  // Varsayýlan pozisyon
        Vector3 endPosition = new Vector3(1, 0, 1);  // Varsayýlan pozisyon

        if (snapToGrid)
        {
            startPosition.x = Mathf.Round(startPosition.x / gridSizeFloat) * gridSizeFloat;
            startPosition.y = Mathf.Round(startPosition.y / gridSizeFloat) * gridSizeFloat;
            startPosition.z = Mathf.Round(startPosition.z / gridSizeFloat) * gridSizeFloat;

            endPosition.x = Mathf.Round(endPosition.x / gridSizeFloat) * gridSizeFloat;
            endPosition.y = Mathf.Round(endPosition.y / gridSizeFloat) * gridSizeFloat;
            endPosition.z = Mathf.Round(endPosition.z / gridSizeFloat) * gridSizeFloat;
        }

        Route route = Instantiate(routePrefab);
        route.name = "Route_" + currentId;
        GameObject startPoint = Instantiate(startPointPrefab, startPosition, Quaternion.identity, route.transform);
        GameObject endPoint = Instantiate(endPointPrefab, endPosition, Quaternion.identity, route.transform);

        if (container != null)
        {
            route.transform.SetParent(container.transform);
        }

        routes.Add(route);

        currentId++;
    }

    private void RemoveRoute(int index)
    {
        if (index >= 0 && index < routes.Count)
        {
            Route routeToRemove = routes[index];
            routes.RemoveAt(index);
            DestroyImmediate(routeToRemove.gameObject);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if ((isPlacing || isDeleting) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Vector2 mousePos = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (isPlacing && selectedObstacle != null)
                {
                    Vector3 position = hit.point;
                    if (snapToGrid)
                    {
                        position.x = Mathf.Round(position.x / gridSizeFloat) * gridSizeFloat;
                        position.y = Mathf.Round(position.y / gridSizeFloat) * gridSizeFloat;
                        position.z = Mathf.Round(position.z / gridSizeFloat) * gridSizeFloat;
                    }
                    lastPlacedObstacle = Instantiate(selectedObstacle, position, Quaternion.Euler(0, rotationAngle, 0));
                    if (lastPlacedObstacle.TryGetComponent<Block>(out Block lastPlacedBlock))
                    {
                        lastPlacedBlock.RenameByBlockSize();
                    }
                    if (container != null)
                    {
                        lastPlacedObstacle.transform.SetParent(container.transform);
                    }
                }
                else if (isDeleting)
                {
                    if (hit.transform.gameObject != lastCreatedGrid)
                    {
                        DestroyImmediate(hit.transform.gameObject);
                    }
                }
            }

            Event.current.Use();
        }
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        routes = new List<Route>();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        routes.Clear();
    }

    private void SaveCurrentLevel()
    {
        LevelData levelData = new LevelData();
        levelData.levelNumber = levelNumber;
        levelData.grids = new List<GridData>();
        levelData.obstacles = new List<ObstacleData>();
        levelData.routes = new List<RouteData>();

        if (lastCreatedGrid != null)
        {
            GridData gridData = new GridData
            {
                position = lastCreatedGrid.transform.position,
                scale = lastCreatedGrid.transform.localScale
            };
            levelData.grids.Add(gridData);
        }

        if (container != null)
        {
            foreach (Transform child in container.transform)
            {
                if (child.gameObject != lastCreatedGrid && !routes.Contains(child.GetComponent<Route>()))
                {
                    if (child != null)
                    {
                        if (child.TryGetComponent<Block>(out Block lastPlacedBlock))
                        {
                            lastPlacedBlock.RenameByBlockSize();
                        }

                        ObstacleData obstacleData = new ObstacleData
                        {
                            name = child.name,
                            position = child.position,
                            rotation = child.rotation.eulerAngles,
                            scale = child.localScale
                        };
                        levelData.obstacles.Add(obstacleData);
                    }
                }
            }

            foreach (Route route in routes)
            {

                RouteData routeData = new RouteData
                {
                    id = int.Parse(route.name.Split('_')[1]),
                    startPoint = new StartPointData
                    {
                        position = route.transform.GetChild(0).position,
                        rotation = route.transform.GetChild(0).rotation.eulerAngles,
                        scale = route.transform.GetChild(0).localScale
                    },
                    endPoint = new EndPointData
                    {
                        position = route.transform.GetChild(1).position,
                        rotation = route.transform.GetChild(1).rotation.eulerAngles,
                        scale = route.transform.GetChild(1).localScale
                    }
                };
                levelData.routes.Add(routeData);
            }
        }

        LevelManager.SaveLevel(levelData, $"Level{levelNumber}");  // Dosya adý
    }

    private void LoadLevel(string fileName)
    {
        LevelData levelData = LevelManager.LoadLevel(fileName);

        if (levelData == null)
        {
            Debug.LogError("Level data not found!");
            return;
        }

        levelNumber = levelData.levelNumber;

        if (levelData.grids.Count > 0)
        {
            if (lastCreatedGrid != null)
            {
                DestroyImmediate(lastCreatedGrid);
            }

            GridData gridData = levelData.grids[0];
            lastCreatedGrid = Instantiate(groundPrefab, gridData.position, Quaternion.identity);
            lastCreatedGrid.transform.localScale = gridData.scale;

            if (container != null)
            {
                lastCreatedGrid.transform.SetParent(container.transform);
            }
        }

        if (container != null)
        {
            foreach (ObstacleData obstacleData in levelData.obstacles)
            {
                GameObject obstaclePrefab = obstacleList.obstacles.ToList().Find(o => o.name == obstacleData.name);
                if (obstaclePrefab != null)
                {
                    GameObject obstacle = Instantiate(obstaclePrefab, obstacleData.position, Quaternion.Euler(obstacleData.rotation), container.transform);
                    obstacle.transform.localScale = obstacleData.scale;
                }
                else
                {
                    Debug.LogWarning($"Obstacle prefab not found for {obstacleData.name}");
                }
            }

            routes.Clear();
            foreach (RouteData routeData in levelData.routes)
            {
                
                Route route = Instantiate(routePrefab);
                route.name = "Route_" + routeData.id;
                GameObject startPoint = Instantiate(startPointPrefab, routeData.startPoint.position, Quaternion.Euler(routeData.startPoint.rotation), route.transform);
                startPoint.transform.localScale = routeData.startPoint.scale;

                GameObject endPoint = Instantiate(endPointPrefab, routeData.endPoint.position, Quaternion.Euler(routeData.endPoint.rotation), route.transform);
                endPoint.transform.localScale = routeData.endPoint.scale;

                route.InitializeRoute(startPoint, endPoint);
                route.transform.SetParent(container.transform);
                routes.Add(route);
                
            }
        }
    }
}
