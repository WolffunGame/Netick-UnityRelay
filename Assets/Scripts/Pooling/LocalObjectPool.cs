using UnityEngine;

public class LocalObjectPool : ObjectPool<MonoBehaviour>
{
    public static LocalObjectPool LocalPool;

    public static T Acquire<T>(T prefab, Vector3 pos = default, Quaternion rot = default, Transform p = default) where T : MonoBehaviour
    {
        if (!Application.isPlaying)
            return null;
        if (LocalPool != null) return (T)LocalPool.AcquireInstance(prefab, pos, rot, p);
        var go = new GameObject("LocalObjectPool");
        DontDestroyOnLoad(go);
        LocalPool = go.AddComponent<LocalObjectPool>();
        return (T) LocalPool.AcquireInstance(prefab, pos,rot,p);
    }

    public static void Release(MonoBehaviour obj) => LocalPool.ReleaseInstance(obj);
}