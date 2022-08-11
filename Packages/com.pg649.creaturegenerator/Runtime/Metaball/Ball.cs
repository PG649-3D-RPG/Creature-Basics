using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball
{

    public float R;
    public Vector3 position;
    public MetaballFunction function;
    public bool inverted;

    public Ball(float r, Vector3 pos, MetaballFunction function, bool inverted = false)
    {
        this.R = r;
        this.position = pos;
        this.function = function;
        this.inverted = inverted;
    }

    public virtual float Value(float x, float y, float z)
    {
        float v = 0f;
        float l2_norm = Mathf.Pow(x - position.x, 2) + Mathf.Pow(y - position.y, 2) + Mathf.Pow(z - position.z, 2);
        switch(function) {
            case MetaballFunction.Polynomial2:
                v = Polynomial(l2_norm, 2);
                break;
            case MetaballFunction.Polynomial3:
                v = Polynomial(l2_norm, 3);
                break;
            case MetaballFunction.Exponential:
                v = Exponential(l2_norm);
                break;
            default:
                throw new Exception("Unrecognized metaball function!");
        }
        if (inverted)
            return -v;
        else
            return v;
    }

    internal float Polynomial(float l2_norm, int p = 2) {
        float r = Mathf.Sqrt(l2_norm);
        return Mathf.Pow(R / r, p);
    }
    
    internal float Exponential(float l2_norm) {
        return Mathf.Exp(0.5f - (0.5f * (l2_norm)) / (R*R));
    }

    public virtual Bounds GetBounds() {
        Bounds bounds = new Bounds(position, new Vector3(R*2, R*2, R*2));
        return bounds;
    }

    public static int GetMinimumNumBalls(MetaballFunction function, float segmentLength, float segmentThickness) {
        switch(function) {
            case MetaballFunction.Polynomial2:
                return Mathf.CeilToInt(segmentLength / (Mathf.Pow(2, 1 / 2.0f) * segmentThickness * 2));
            case MetaballFunction.Polynomial3:
                return Mathf.CeilToInt(segmentLength / (Mathf.Pow(2, 1 / 3.0f) * segmentThickness * 2));
            case MetaballFunction.Exponential:
                return Mathf.CeilToInt(segmentLength / (2.97f * segmentThickness  ));
            default:
                throw new Exception("Unrecognized metaball function!");
        }
    }

}
