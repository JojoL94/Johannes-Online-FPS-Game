using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Vector3 GetRandomSpawnPoint()
    {
        return new Vector3(Random.Range(-6, 6), 15, Random.Range(-10, 10));
    }

    public static void SetRenderLayerInChildren(Transform transform, int layerNumber)
    {
        foreach (Transform trans in transform.GetComponentsInChildren<Transform>(true))
        {
            if (trans.CompareTag("IgnoreLayerChange"))
                continue;

            trans.gameObject.layer = layerNumber;
        }
    }
}
