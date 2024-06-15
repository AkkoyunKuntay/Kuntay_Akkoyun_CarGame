using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Route : MonoBehaviour
{
    public RouteProperties routeProperty;

    private void Start()
    {
        RouteFlowController.instance.RegisterRouteToList(this);
        InstantiateSpawnee();
        gameObject.SetActive(false); 
    }

    public void InitializeRoute(GameObject start, GameObject end)
    {
        routeProperty.StartPoint = start.transform;
        routeProperty.EndPoint = end.transform;
    }

    private void InstantiateSpawnee()
    {
        GameObject spawnedCar = Instantiate(routeProperty.spawnee, routeProperty.StartPoint.position, routeProperty.StartPoint.rotation);
        spawnedCar.GetComponent<CarController>().InitializeCar(routeProperty.StartPoint, routeProperty.EndPoint, routeProperty.TimeForRoute);
        routeProperty.spawnedVehicle = spawnedCar;
        Debug.Log("Car Initialized!");
    }

    public void EnableRouteVisibility(bool status)
    {
        routeProperty.EndPoint.GetComponentInChildren<MeshRenderer>().enabled= status;
        routeProperty.StartPoint.GetComponentInChildren<MeshRenderer>().enabled = status;
        routeProperty.spawnedVehicle.SetActive(status);
    }
}

[System.Serializable]
public class RouteProperties
{
    public Transform StartPoint;
    public Transform EndPoint;
    public float TimeForRoute;
    public GameObject spawnee;
    [HideInInspector] public GameObject spawnedVehicle;
}
