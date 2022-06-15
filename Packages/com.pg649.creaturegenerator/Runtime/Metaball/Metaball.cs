using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metaball
{
    private List<Ball> balls = new List<Ball>();

    public Metaball() { }

    public void AddBall(float radius, Vector3 position, MetaballFunction function)
    {
        Ball newBall = new Ball(radius, position, function);
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
    /// <param name="function">The MetaballFunction to use</param>
    /// <returns>A Metaball made up of balls distributed along the provided segments</returns>
    public static Metaball BuildFromSegments(Segment[] segments, MetaballFunction function = MetaballFunction.Polynomial2, float variation=0.75f)
    {
        Metaball metaball = new Metaball();

        foreach (Segment segment in segments)
        {
            int numBalls = Ball.GetMinimumNumBalls(function, segment.GetLength(), segment.thickness);

            Vector3 fwd = segment.GetEndPoint() - segment.GetStartPoint();
            Vector3 toMidPoint = fwd / (2 * numBalls);

            for (float i = 0; i <= numBalls; i++)
            {
                Vector3 position = segment.GetStartPoint() + (i / numBalls) * fwd;
                Vector3 randomDirection = new Vector3(RandomGaussian(-toMidPoint.magnitude, toMidPoint.magnitude),
                    RandomGaussian(-toMidPoint.magnitude, toMidPoint.magnitude),
                    RandomGaussian(-toMidPoint.magnitude, toMidPoint.magnitude)) * variation;
                metaball.AddBall(RandomGaussian(segment.thickness * 0.5f, segment.thickness * 1.5f) * variation, position + toMidPoint + randomDirection, function);
                metaball.AddBall(segment.thickness, position, function);
            }
        }
        return metaball;
    }

    public static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
    {
        float u, v, S;

        do
        {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }
}
