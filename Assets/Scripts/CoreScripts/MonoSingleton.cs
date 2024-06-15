using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : Component
{
    public static T instance;

    protected virtual void Awake()
    {
        if (instance != null & instance != this)
        {
            Debug.Log("Duplicate singleton found for " + instance.name + ". Destroying this: " + gameObject.name);
            Destroy(gameObject);
        }

        else
        {
            instance = this as T;
        }
    }
}