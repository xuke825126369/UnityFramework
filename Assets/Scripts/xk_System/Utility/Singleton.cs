using UnityEngine;
using System;

/// <summary>
/// 如果实现单例，就继承这个类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Singleton<T> where T : class, new()
{
    private static T instance = new T();
    public static T Instance
    {
        get
        {
            return instance;
        }
    }

    public virtual void Init()
	{

	}
}

public abstract class SingleTonMonoBehaviour<T> : MonoBehaviour where T : SingleTonMonoBehaviour<T>
{
    private static T instance = null;
    public static T Instance
    {
        get
        {
            if (null == instance)
            {
                instance = GameObject.FindObjectOfType<T>();
                if (instance == null)
                {
                    GameEngine parent = GameObject.FindObjectOfType<GameEngine>();
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    if (obj.GetComponent<T>() == null)
                    {
                        instance = obj.AddComponent<T>();
                    }
                    obj.transform.SetParent(parent.transform);
                }
            }
            return instance;
        }
    }

    public virtual void Init()
    {

    }
}

