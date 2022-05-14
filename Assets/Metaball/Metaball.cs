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

    /// <summary>
    /// Builds a metaball from the provided segments
    /// </summary>
    /// <param name="segments">Array of segments</param>
    /// <returns>A Metaball made up of balls distributed along the provided segments</returns>
    public static Metaball BuildFromSegments(Segment[] segments)
    {
        Metaball metaball = new Metaball();

        foreach (Segment segment in segments)
        {
            //polynomial solution
            //int numBalls = Mathf.CeilToInt(segment.GetLength() / (Mathf.Pow(2, 0.5f) * segment.thickness * 2));

            //exponential solution
            int numBalls = Mathf.CeilToInt(segment.GetLength() / (2.97f * segment.thickness));

            Vector3 fwd = segment.endPoint - segment.startPoint;

            for (float i = 0; i <= numBalls; i++)
            {
                metaball.AddBall(segment.thickness, segment.startPoint + (i / numBalls) * fwd);
            }
        }
        return metaball;
    }
}
