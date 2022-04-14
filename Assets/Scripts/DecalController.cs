using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalController : MonoBehaviour
{
    // based on https://unity3d.college/2017/02/28/pooled-decals-bullet-holes/

    public int maxNumOfDecals = 10;

    public float displacement = 0.001f;

    public GameObject decalPrefab;

    private Queue<GameObject> decals;

    private void Start()
    {
        decals = new Queue<GameObject>();
    }

    public void spawnDecal(RaycastHit hit)
    {
        GameObject newDecal = Instantiate(decalPrefab, hit.point + hit.normal * displacement, Quaternion.FromToRotation(Vector3.up, hit.normal));

        if (!hit.transform.gameObject.isStatic)
        {
            newDecal.transform.SetParent(hit.transform, true);
        }

        newDecal.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

        decals.Enqueue(newDecal);

        if(decals.Count > maxNumOfDecals)
        {
            Destroy(decals.Dequeue());
        }
    } 
}
