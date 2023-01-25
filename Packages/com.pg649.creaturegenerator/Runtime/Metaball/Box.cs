using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

public class Box: Ball
{
    public Vector3 dimensions;
    public Vector3 fwd;
    public Quaternion toLocalAxes;
    public Vector3 center;

    public Box(Vector3 dimensions, Vector3 pos, Vector3 fwd, Vector3 up, FalloffFunction function, Bone? bone=null) : base(dimensions.x, Vector3.zero, function, bone: bone)
    {
        this.fwd = fwd;
        // dimensions now represent distance from center to edge of box
        this.dimensions = dimensions / 2f;

        // use center of box as position
        center = pos + fwd.normalized * this.dimensions.z;

        toLocalAxes = Quaternion.Inverse(Quaternion.LookRotation(fwd, up));

        Debug.Log("pos " + this.center);
        Debug.Log("dim " + this.dimensions);
    }

    public override float Value(float x, float y, float z)
    {
        // transform point to local coordinates
        Vector3 point = new Vector3(x, y, z) - center;
        point = toLocalAxes * point;
        point.x /= dimensions.x;
        point.y /= dimensions.y;
        point.z /= dimensions.z;

        float dist = Mathf.Max(Mathf.Abs(point.x), Mathf.Abs(point.y), Mathf.Abs(point.z));

        return base.Value(dist, 0, 0);
    }
    
    public override Bounds GetBounds() {
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int x=-1; x<=1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 corner = new Vector3(x, y, z);
                    corner.Scale(dimensions);
                    corner = Quaternion.Inverse(toLocalAxes) * corner;

                    max = Vector3.Max(max, corner);
                }
            }
        }
        Bounds bounds = new Bounds(center, max*2f);

        return bounds;
    }

}
