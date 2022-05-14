using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metaball
{
    private List<Ball> balls = new List<Ball>();
    public Metaball() { }

    public void AddBall(float radius, Vector3 position)
    {
        Ball newBall = new Ball(radius, position);
        balls.Add(newBall);
    }

    public float Value(float x, float y, float z)
    {
        float result = 0f;
        foreach(Ball ball in balls)
        {
            result += ball.Value(x, y, z);
        }
        return result;
    }
}
