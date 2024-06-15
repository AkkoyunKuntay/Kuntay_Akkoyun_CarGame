using UnityEngine;
using UnityEngine.EventSystems;

public class TurnButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isRightTurnButton;
    public float turnSpeed;
    private bool isPressed = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    private void Update()
    {
        if (isPressed && RouteFlowController.instance.activeVehicle != null )
        {
            
            if (isRightTurnButton)
            {
                RouteFlowController.instance.activeVehicle.transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
            }
            else
            {
                RouteFlowController.instance.activeVehicle.transform.Rotate(0, -turnSpeed * Time.deltaTime, 0);
            }
        }
    }
}
