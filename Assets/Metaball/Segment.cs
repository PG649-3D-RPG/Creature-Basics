using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment
{
    public float thickness;
    public Vector3 startPoint;
    public Vector3 endPoint;

    public Segment(Vector3 start, Vector3 end, float thickness)
    {
        startPoint = start;
        endPoint = end;
        this.thickness = thickness;
    }

    public Vector3 GetStartPoint() {
        return startPoint;
    }

    public Vector3 GetEndPoint() {
        return endPoint;
    }

    public float GetLength()
    {
        return (endPoint - startPoint).magnitude;
    }

}
