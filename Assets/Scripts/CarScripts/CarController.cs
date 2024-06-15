using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour, ICrushable
{
    [SerializeField] carData CarData;
    private bool isMoving = false;
    public bool isReplaying = false;
    private float defaultTime;
    public List<PathPoint> path;
    private int pathIndex = 0;
    private bool nextRouteFlag=false;

    #region ICrushable Region
    public void DetectCollision(Collider other)
    {
        if (other == null) return;
        if (other.TryGetComponent<Block>(out Block block))
        {
            Crush(block.transform);
        }
        if (other.TryGetComponent<CarController>(out CarController car))
        {
            Crush(car.transform);
        }
    }

    public void Crush(Transform crushedWith)
    {
        path.Clear();   
        Debug.Log(gameObject.name + " is Crushed with : " + crushedWith);
        StopMoving();
        RouteFlowController.instance.ResetActiveRoute();
        
    }
    #endregion

    private void Start()
    {
        RouteFlowController.instance.CarActivatedEvent += OnCarActivatedEvent;
        StartCoroutine(RecordPath());
    }

    private void OnDestroy()
    {
        RouteFlowController.instance.CarActivatedEvent -= OnCarActivatedEvent;
    }

    private void OnCarActivatedEvent(GameObject activeVehicle)
    {
        if (activeVehicle == gameObject)
        {
            Debug.Log("Car Activated: " + gameObject.name);
        }
    }

    public void InitializeCar(Transform start, Transform end, float time)
    {
        CarData.startPoint = start;
        CarData.endPoint = end;
        CarData.timeforRoute = time;
        defaultTime = time;
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    private void StopMoving()
    {
        isMoving = false;
        isReplaying = false;
    }

    public void ResetTimer()
    {
        CarData.timeforRoute = defaultTime;
    }

    private void Update()
    {
        if (isMoving)
        {
            if (isReplaying)
            {
                FollowPath();
            }
            else
            {
                MoveCar();
            }
        }
    }

    private void MoveCar()
    {
        CarData.timeforRoute -= Time.deltaTime;
        if (CarData.timeforRoute > 0)
        {
            transform.position += transform.forward * 2.5f * Time.deltaTime;
        }
        else
        {
            StopMoving();
            RouteFlowController.instance.ResetActiveRoute();
        }
    }

    private IEnumerator RecordPath()
    {
        while (true)
        {
            if (isMoving && !isReplaying)
            {
                path.Add(new PathPoint(transform.position, transform.rotation, CarData.timeforRoute));
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void FollowPath()
    {
        if (pathIndex < path.Count)
        {
            PathPoint nextPoint = path[pathIndex];
            Vector3 direction = nextPoint.position - transform.position;
            transform.position += direction.normalized * 3f * Time.deltaTime;

            float step = Time.deltaTime * 10f;
            transform.rotation = Quaternion.Lerp(transform.rotation, nextPoint.rotation, step);

            if (Vector3.Distance(transform.position, nextPoint.position) < 0.01f)
            {
                pathIndex++;
            }
        }
        else
        {
            StopMoving();

            ResetCar();
        }
    }

    public void StartReplay()
    {
        pathIndex = 0;
        isReplaying = true;
        StartMoving();
    }

    public IEnumerator StopReplay()
    {
        CarController activeVehicle = RouteFlowController.instance.activeVehicle.GetComponent<CarController>();
        yield return new WaitUntil(() => activeVehicle.nextRouteFlag);

        isReplaying = false;
        StopMoving();
    }

    public void ResetCar()
    {
        transform.GetComponent<Collider>().enabled = true;
        transform.position = CarData.startPoint.position;
        transform.rotation = CarData.startPoint.rotation;
        ResetTimer();
        nextRouteFlag = false;
        StartCoroutine(StopReplay());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == CarData.endPoint)
        {
            nextRouteFlag = true;
            transform.GetComponent<Collider>().enabled = false;
            StopMoving();
            ResetCar();
            RouteFlowController.instance.StartNextRoute();
        }
        else
        {
            DetectCollision(other);
        }
    }
}

[System.Serializable]
public struct carData
{
    public Transform startPoint;
    public Transform endPoint;
    public float timeforRoute;
}
[System.Serializable]
public class PathPoint
{
    public Vector3 position;
    public Quaternion rotation;
    public float remainingTime;

    public PathPoint(Vector3 pos, Quaternion rot, float time)
    {
        position = pos;
        rotation = rot;
        remainingTime = time;
    }
}
