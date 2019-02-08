using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ns3dRudder;

public class Manager3dRudder : MonoBehaviour
{
    private static Manager3dRudder instance;
    public static Manager3dRudder Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Manager3dRudder>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(Manager3dRudder).Name;
                    obj.hideFlags = HideFlags.HideInHierarchy;
                    instance = obj.AddComponent<Manager3dRudder>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    public bool IsInitialized = false;

    // Use this for initialization
    void Awake ()
    {
#if !UNITY_EDITOR
        Sdk3dRudder.Initialize();
#endif
        IsInitialized = true;
    }

    void OnApplicationQuit()
    {
#if !UNITY_EDITOR
        Sdk3dRudder.Stop();
#endif
    }
}
