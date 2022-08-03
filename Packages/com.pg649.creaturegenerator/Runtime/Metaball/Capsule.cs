using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capsule: Ball
{
    public Segment segment;

    public Capsule(Segment seg, MetaballFunction function) : base(seg.thickness, Vector3.zero, function)
    {
        segment = seg;
    }

    public override float Value(float x, float y, float z)
    {
        // calculate distance from point to segment center line
        Vector3 testPoint = new Vector3(x, y, z);

        Vector3 segmentFwd = segment.endPoint - segment.startPoint;
        Vector3 startToPoint = testPoint - segment.startPoint;
        Vector3 endToPoint = testPoint - segment.endPoint;

        float dotStart = Vector3.Dot(segmentFwd, startToPoint);
        float dotEnd = Vector3.Dot(segmentFwd, endToPoint);

        float dist;
        if (dotEnd > 0)
        {
            // Finding the magnitude
            Vector3 offset = testPoint - segment.endPoint;
            dist = offset.magnitude;
        }
        else if (dotStart < 0) // Case 2
        {
            Vector3 offset = testPoint - segment.startPoint;
            dist = offset.magnitude;
        }
        else // Case 3
        {
            // Finding the perpendicular distance
            float mod = segmentFwd.magnitude;
            dist = (Vector3.Cross(segmentFwd, startToPoint) / mod).magnitude;
        }

        return base.Value(dist, 0, 0);
    }
    
    public override Bounds GetBounds() {
        Bounds bounds = new Bounds(segment.startPoint, new Vector3(segment.thickness*2, segment.thickness*2, segment.thickness*2));
        bounds.Encapsulate(new Bounds(segment.endPoint, new Vector3(segment.thickness*2, segment.thickness*2, segment.thickness*2)));

        bounds.Expand(0.75f); //TODO only temporary fix...

        return bounds;
    }

}
