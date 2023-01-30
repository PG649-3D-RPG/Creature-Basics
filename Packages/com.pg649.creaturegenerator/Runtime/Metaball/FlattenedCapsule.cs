using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

public class FlattenedCapsule : Ball
{
    Segment segment;
    Vector3 axis;
    float width;
    public FlattenedCapsule(Segment seg, Vector3 scaleAxis, float width, FalloffFunction function, Bone? bone=null) : base(seg.thickness, Vector3.zero, function, bone: bone)
    {
        segment = new(seg.startPoint, seg.endPoint, seg.thickness);
        Vector3 segFwd = segment.endPoint - segment.startPoint;
        if (segment.GetLength() > 2f * segment.thickness)
        {
            segment.startPoint += segment.thickness * 0.5f * segFwd;
            segment.endPoint -= segment.thickness * 0.5f * segFwd;
        }
        else
        {
            segment.startPoint += 0.25f * segFwd;
            segment.endPoint -= 0.25f * segFwd;
        }
        axis = scaleAxis.normalized;
        axis = axis.Abs();
        this.width = width;
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
        Vector3 offset;
        float scaleFactor =  segment.thickness / width - 1f;
        if (dotEnd > 0)
        {
            // Finding the magnitude
            offset = testPoint - segment.endPoint;
        }
        else if (dotStart < 0) // Case 2
        {
            offset = testPoint - segment.startPoint;
        }
        else // Case 3
        {
            // Finding the perpendicular vector from segment line to point
            Vector3 nearestPoint = segment.startPoint + (Vector3.Dot(startToPoint, segmentFwd) / segmentFwd.magnitude) * segmentFwd.normalized;
            offset = testPoint - nearestPoint;
        }
        offset.Scale(new Vector3(1, 1, 1) + axis * scaleFactor);
        dist = offset.magnitude;
        return base.Value(dist, 0, 0);
    }

    public override Bounds GetBounds()
    {
        float maxEdge = 2f * Mathf.Max(segment.thickness, width);
        Bounds bounds = new Bounds(segment.startPoint, new Vector3(maxEdge, maxEdge, maxEdge));
        bounds.Encapsulate(new Bounds(segment.endPoint, new Vector3(maxEdge, maxEdge, maxEdge)));

        return bounds;
    }
}
