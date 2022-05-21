using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball
{

    public float R;
    public Vector3 position;
    public MetaballFunction function;

    public Ball(float r, Vector3 pos, MetaballFunction function)
    {
        R = r;
        position = pos;
        this.function = function;
    }

    public float Value(float x, float y, float z)
    {
        switch(function) {
            case MetaballFunction.Polynomial2:
                return Polynomial(x, y, z, 2);
            case MetaballFunction.Polynomial3:
                return Polynomial(x, y, z, 3);
            case MetaballFunction.Exponential:
                return Exponential(x, y, z);
            default:
                throw new Exception("Unrecognized metaball function!");
        }
    }

    private float Polynomial(float x, float y, float z, int p = 2) {
        float r = Mathf.Sqrt(Mathf.Pow(x - position.x, 2) + Mathf.Pow(y - position.y, 2) + Mathf.Pow(z - position.z, 2));
        return Mathf.Pow(R, p)/Mathf.Pow(r, p);
    }
    
    private float Exponential(float x, float y, float z) {
        return Mathf.Exp(0.5f - (0.5f * (Mathf.Pow(x - position.x, 2) + Mathf.Pow(y - position.y, 2) + Mathf.Pow(z - position.z, 2))) / (R*R));
    }

}
