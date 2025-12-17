using System;
using UnityEngine;
using Logger = LenixSO.Logger.Logger;

public class TestScript : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Initialize()
    {
        new GameObject("Test").AddComponent<TestScript>();
    }

    private void Start()
    {
        //Logger.Log("test1", LogFlags.Flag1);
        //Logger.Log("test2", LogFlags.Flag2);
        //Debug.Log("test default");
        //Debug.unityLogger.logHandler = new TestLogger(Debug.unityLogger.logHandler);
        //Debug.Log("test handler");
        Debug.Log("regular log");
        Logger.Log("logger log"); 
    }
}