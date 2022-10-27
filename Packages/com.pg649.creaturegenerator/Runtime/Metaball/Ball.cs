using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball
{

    public float R;
    public Vector3 position;
    public FalloffFunction function;
    public bool inverted;

    public Ball(float r, Vector3 pos, FalloffFunction function, bool inverted = false)
    {
        this.R = r;
        this.position = pos;
        this.function = function;
        this.inverted = inverted;
    }

    public virtual float Value(float x, float y, float z)
    {
        float l2_norm = Mathf.Pow(x - position.x, 2) + Mathf.Pow(y - position.y, 2) + Mathf.Pow(z - position.z, 2);
        float v = function.Calc(l2_norm, R);

        if (inverted)
            return -v;
        else
            return v;
    }

    public virtual Bounds GetBounds() {
        Bounds bounds = new Bounds(position, new Vector3(R*2, R*2, R*2));
        return bounds;
    }

    public static int GetMinimumNumBalls(FalloffFunction function ,float segmentLength, float segmentThickness) {
        return function.GetMinimumNumBalls(segmentLength, segmentThickness);
    }

}
