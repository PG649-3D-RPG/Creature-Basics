using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour
{
    public float thickness;

    [HideInInspector]
    public Vector3 startPoint;
    [HideInInspector]
    public Vector3 endPoint;
    // Start is called before the first frame update
    void Start()
    {
        startPoint = this.transform.localPosition;
        endPoint = startPoint + transform.localRotation * Vector3.forward * transform.localScale.z;
        GetComponent<LineRenderer>().enabled = false;
    }

    public float GetLength()
    {
        return transform.localScale.z;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
