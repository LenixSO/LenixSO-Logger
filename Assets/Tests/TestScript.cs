using System;
using UnityEngine;
using LenixSO.Logger;
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

        Debug.Log("regular log");
        Logger.Log("logger log"); 
    }
}