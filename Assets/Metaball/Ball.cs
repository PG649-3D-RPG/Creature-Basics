using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball
{
    public float R;
    public Vector3 position;
    public Ball(float r, Vector3 pos)
    {
        R = r;
        position = pos;
    }

    public float Value(float x, float y, float z)
    {
        // polynomial solution
        // float r = Mathf.Sqrt(Mathf.Pow(x - position.x, 2) + Mathf.Pow(y - position.y, 2) + Mathf.Pow(z - position.z, 2));
        //return Mathf.Pow(R, 2f)/Mathf.Pow(r, 2f);

        // exponential solution
        return Mathf.Exp(0.5f - (0.5f * (Mathf.Pow(x - position.x, 2) + Mathf.Pow(y - position.y, 2) + Mathf.Pow(z - position.z, 2))) / (R*R));
    }
}
