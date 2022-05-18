using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour
{
    public float thickness;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<LineRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public Vector3 GetStartPoint() {
        return this.transform.localPosition;
    }

    public Vector3 GetEndPoint() {
        return this.transform.localPosition + transform.localRotation * Vector3.forward * transform.localScale.z;
    }

    public float GetLength()
    {
        return transform.localScale.z;
    }

}
