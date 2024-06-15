using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteFlowController : MonoSingleton<RouteFlowController>
{
    public List<Route> routeList;
    private int routeIndex = 0;

    [Header("Debug")]
    public Route activeRoute;
    public GameObject activeVehicle;

    private List<GameObject> replayVehicles = new List<GameObject>(); // Replay yapan araçlarýn listesi

    public event System.Action<GameObject> CarActivatedEvent;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (routeList != null && routeList.Count > 0)
        {
            InitializeFirstRoute();
        }
    }

    public void RegisterRouteToList(Route route)
    {
        if (routeList == null) return;
        if (routeList.Contains(route)) return;
        routeList.Add(route);
    }

    private void InitializeFirstRoute()
    {
        activeRoute = routeList[0];
        activeVehicle = activeRoute.routeProperty.spawnedVehicle;
        SetActiveRoute();
    }

    private void SetActiveRoute()
    {
        for (int i = 0; i < routeList.Count; i++)
        {
            routeList[i].gameObject.SetActive(i == routeIndex);

        }

        if (activeRoute != null)
        {
            activeVehicle = activeRoute.routeProperty.spawnedVehicle;
            CarActivatedEvent?.Invoke(activeVehicle);
        }

        StartCoroutine(WaitForInput());
    }

    private IEnumerator WaitForInput()
    {
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        activeVehicle.GetComponent<CarController>().StartMoving();

        // Replay yapacak tüm araçlarýn replay fonksiyonunu burada baþlatýyoruz
        foreach (var vehicle in replayVehicles)
        {
            vehicle.GetComponent<CarController>().StartReplay();
        }
    }

    public void StartNextRoute()
    {
        if (routeIndex < routeList.Count - 1)
        {
            // Önceki aracý replay listesine ekle ve resetle
            if (activeVehicle != null)
            {
                if (!replayVehicles.Contains(activeVehicle))
                {
                    replayVehicles.Add(activeVehicle);
                }
                
            }

            routeIndex++;
            activeRoute = routeList[routeIndex];
            activeVehicle = activeRoute.routeProperty.spawnedVehicle;

            SetActiveRoute();
        }
        else
        {
            GameManager.instance.EndGame(true);
            Debug.Log("Tüm rotalar tamamlandý. Oyun kazanýldý!");
        }
    }

    public void ResetActiveRoute()
    {
        if (activeRoute != null)
        {
            activeVehicle.transform.position = activeRoute.routeProperty.StartPoint.position;
            activeVehicle.transform.rotation = activeRoute.routeProperty.StartPoint.rotation;
            activeVehicle.GetComponent<CarController>().ResetTimer();

            // Replay yapan araçlarý resetle
            ResetReplayVehicles();

            StartCoroutine(WaitForInput());

            // Replay yapacak tüm araçlarýn replay fonksiyonunu burada baþlatýyoruz
            foreach (var vehicle in replayVehicles)
            {
                vehicle.GetComponent<CarController>().StartReplay();
            }

        }
    }

    public void ResetReplayVehicles()
    {
        foreach (var vehicle in replayVehicles)
        {
            vehicle.GetComponent<CarController>().ResetCar();
            vehicle.GetComponent<CarController>().StopReplay();
        }
    }
}
