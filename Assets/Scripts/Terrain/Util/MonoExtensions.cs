using UnityEngine;
using Component = UnityEngine.Component;

public static class MonoExtensions
{
    public static bool HasComponent<T>(this Transform tf) where T : Component
    {
        if (tf.GetComponent<T>() != null)
        {
            return true;
        }
        return false;
    }

    public static bool HasComponent<T>(this GameObject go) where T : Component
    {
        if (go == null)
        {
            return false;
        }
        return go.GetComponent<T>() != null;
    }
    public static bool HasComponent<T>(this Component mb) where T: Component
    {
        return mb.GetComponent<T>() != null;
    }

    public static bool ObtainComponent<T>(this Component mb) where T : Component
    {
        var component = mb.GetComponent<T>();
        if (component == null)
        {
            mb.gameObject.AddComponent<T>();
            return ObtainComponent<T>(mb);
        }
        return component;
    }

    public static Vector3 XY(this Vector3 v3) => new Vector3(v3.x, v3.y);
    
    public static Vector3 YZ(this Vector3 v3) => new Vector3(0, v3.y,v3.z);
    
    public static Vector3 XZ(this Vector3 v3) => new Vector3(v3.x, 0,v3.z);
    
    public static Vector3 X(this Vector3 v3) => new Vector3(v3.x, 0,0);
    
    public static Vector3 Y(this Vector3 v3) => new Vector3(0,v3.y, 0);
    
    public static Vector3 Z(this Vector3 v3) => new Vector3(0,0, v3.z);
    

    public static T ObtainComponent<T>(this GameObject go) where T : MonoBehaviour
    {
        return go.HasComponent<T>()
            ? go.GetComponent<T>()
            : go.AddComponent<T>();
    }

    public static bool HasComponent<T>(this MonoBehaviour mb) where T : Component
    {
        if (mb.GetComponent<T>() != null)
        {
            return true;
        }
        return false;
    }

    public static T ObtainComponent<T>(this MonoBehaviour mb) where T : MonoBehaviour
    {
        return mb.HasComponent<T>()
            ? mb.GetComponent<T>()
            : mb.gameObject.AddComponent<T>();
    }
}
