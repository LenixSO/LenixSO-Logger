using LenixSO.Logger;
using UnityEngine;
using Logger = LenixSO.Logger.Logger;

public class TestScript : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Initialize()
    {
        Logger.Log("test1", LogFlags.Flag1);
        Logger.Log("test2", LogFlags.Flag2);
    }
}
