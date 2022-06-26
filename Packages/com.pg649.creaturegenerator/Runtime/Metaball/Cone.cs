using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cone: Ball
{
    public Segment segment;
    public float ratio;

    public Cone(Segment seg, float tipThickness, MetaballFunction function) : base(seg.thickness, Vector3.zero, function)
    {
        segment = seg;
        ratio = seg.thickness / tipThickness;
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
            dist = offset.magnitude * ratio;
        }

        // Case 2
        else if (dotStart < 0)
        {
            Vector3 offset = testPoint - segment.startPoint;
            dist = offset.magnitude;
        }

        // Case 3
        else
        {

            // Finding the perpendicular distance
            float mod = segmentFwd.magnitude;
            float perpendicular = (Vector3.Cross(segmentFwd, startToPoint) / mod).magnitude;
            float fwdDist = Mathf.Sqrt(MathF.Pow(startToPoint.magnitude, 2) - perpendicular * perpendicular);
            dist = perpendicular * ((fwdDist / segmentFwd.magnitude) * (ratio - 1f) + 1f);
        }

        return base.Value(dist, 0, 0);
    }

}
