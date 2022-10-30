using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metaball
{
    private List<Ball> balls = new List<Ball>();

    public Metaball() { }

    public void AddBall(Ball ball)
    {
        balls.Add(ball);
    }

    public void AddBall(float radius, Vector3 position, FalloffFunction function)
    {
        Ball newBall = new Ball(radius, position, function);
        balls.Add(newBall);
    }

    public void AddCapsule(Segment segment, FalloffFunction function)
    {
        Capsule newCapsule = new Capsule(segment, function);
        balls.Add(newCapsule);
    }

    public void AddCone(Segment segment, float tipThickness, FalloffFunction function)
    {
        Cone newCone = new Cone(segment, tipThickness, function);
        balls.Add(newCone);
    }

    public void AddBox(Vector3 dimensions, Vector3 position, Vector3 forward, Vector3 up, FalloffFunction function)
    {
        Box newBox = new Box(dimensions, position, forward, up, function);
        balls.Add(newBox);
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

    public Bounds GetBounds() {
        if (balls.Count == 0)
            return new Bounds(Vector3.zero, Vector3.zero);
        
        Bounds bounds = balls[0].GetBounds();
        for (int i = 1; i < balls.Count; i++) {
            bounds.Encapsulate(balls[i].GetBounds());
        }

        return bounds;
    }

    /// <summary>
    /// Builds a metaball from the provided segments
    /// </summary>
    /// <param name="segments">Array of segments</param>
    /// <param name="function">The MetaballFunction to use</param>
    /// <returns>A Metaball made up of balls distributed along the provided segments</returns>
    public static Metaball BuildFromSegments(Segment[] segments, FalloffFunction function, float variation=0.75f, bool useCapsules= true)
    {
        Metaball metaball = new Metaball();

        if (useCapsules)
        {
            foreach (Segment segment in segments)
            {
                metaball.AddCapsule(segment, function);
            }
        }
        else
        {
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
                    metaball.AddBall(Mathf.Abs(RandomGaussian(0.5f, 1.5f) * variation) * segment.thickness, position + randomDirection, function);
                    metaball.AddBall(segment.thickness, position, function);
                }
            }
        }
        return metaball;
    }

    public static Metaball BuildFromSkeleton(Skeleton skeleton, FalloffFunction function) {
        Metaball metaball = new Metaball();
        foreach (var (go, bone, rb, joint) in skeleton.Iterator()) {
            if (bone.category == BoneCategory.Foot && bone.subCategory != BoneCategory.Paw)
            {
                metaball.AddBox(new(bone.thickness, 0.25f, bone.length), bone.WorldProximalPoint(), bone.WorldDistalAxis(), bone.WorldVentralAxis(), function);
            }
            else
            {
                metaball.AddCapsule(new(bone.WorldProximalPoint(), bone.WorldDistalPoint(), bone.thickness), function);
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
