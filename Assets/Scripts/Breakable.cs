using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public GameObject broken;

    public Vector3 normalScale;
    public Vector3 brokenNormalScale;

    public Vector3 rotationAfterBroken;

    public float breakingImpulse = 100f;

    public void Break()
    {
        Vector3 brokenScale = Vector3.Scale(new Vector3(transform.localScale.x / normalScale.x, transform.localScale.y / normalScale.y, transform.localScale.z / normalScale.z), brokenNormalScale);

        Instantiate(broken, transform.position, transform.rotation * Quaternion.Euler(rotationAfterBroken)).transform.localScale = brokenScale;

        Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.impulse.magnitude >= breakingImpulse)
        {
            Break();
        }
    }
}
