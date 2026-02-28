using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    private static T _instance;

    virtual protected void Awake()
    {
        if (_instance == null && _instance != this)
            _instance = this as T;
        else
            Destroy(this.gameObject);
    }

    public static T Instance
    {
        get
        {
            if (_instance is null)
                Debug.LogWarning("Singleton is NULL");
            return _instance;
        }

        protected set { }
    }
}