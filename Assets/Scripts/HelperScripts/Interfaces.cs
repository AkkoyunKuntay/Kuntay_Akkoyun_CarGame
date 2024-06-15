
using UnityEngine;

public class Interfaces : MonoBehaviour
{
    
}
public interface ICrushable
{
    public void DetectCollision(Collider other);
    public void Crush(Transform crushedWith);
}
public interface IButton
{
    public void OnPressed();
}

