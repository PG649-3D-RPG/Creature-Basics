using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#nullable enable

public class Metaball
{
    private List<Ball> balls = new List<Ball>();

    public Metaball() { }

    public void AddBall(Ball ball)
    {
        balls.Add(ball);
    }

    public void AddBall(float radius, Vector3 position, FalloffFunction function, Bone? bone=null, Color? color=null)
    {
        Ball newBall = new Ball(radius, position, function, bone: bone);
        if (color.HasValue)
            newBall.color = color;
        balls.Add(newBall);
    }

    public void AddCapsule(Segment segment, FalloffFunction function, Bone? bone=null, Color? color = null)
    {
        Capsule newCapsule = new Capsule(segment, function, bone: bone);
        if (color.HasValue)
            newCapsule.color = color;
        balls.Add(newCapsule);
    }

    public void AddFlattenedCapsule(Segment segment, Vector3 scaleAxis, float width, FalloffFunction function, Bone? bone = null, Color? color = null)
    {
        FlattenedCapsule newCapsule = new FlattenedCapsule(segment, scaleAxis, width, function, bone: bone);
        if (color.HasValue)
            newCapsule.color = color;
        balls.Add(newCapsule);
    }

    public void AddCone(Segment segment, float tipThickness, FalloffFunction function, Bone? bone = null, Color? color = null)
    {
        Cone newCone = new Cone(segment, tipThickness, function, bone: bone);
        if (color.HasValue)
            newCone.color = color;
        balls.Add(newCone);
    }

    public void AddBox(Vector3 dimensions, Vector3 position, Vector3 forward, Vector3 up, FalloffFunction function, Bone? bone = null, Color? color = null)
    {
        Box newBox = new Box(dimensions, position, forward, up, function, bone: bone);
        if (color.HasValue)
            newBox.color = color;
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

    public (Ball[], float[]) GetWeights(float x, float y, float z)
    {
        Dictionary<Ball, float> ballValues = new();
        float total = 0f;
        foreach (Ball ball in balls)
        {
            float val = ball.Value(x, y, z);
            total += val;
            ballValues.Add(ball, val);
        }
        ballValues = ballValues.Where(bv => bv.Value / total > 0.1 && (bv.Key.bone == null || (bv.Key.bone.category != BoneCategory.Shoulder || bv.Value > 0.6)) ).ToDictionary(bv => bv.Key, bv => bv.Value);
        Ball[] infBalls = new Ball[ballValues.Count];
        float[] weights = new float[ballValues.Count];
        int i = 0;
        foreach(var bv in ballValues)
        {
            infBalls[i] = bv.Key;
            //weights[i] = bv.Value;
            weights[i] = Mathf.Pow(bv.Value, 4f);
            i++;
        }
        Array.Sort(weights, infBalls);
        Array.Reverse(infBalls);
        Array.Sort(weights, new Comparison<float>((i1, i2) => i2.CompareTo(i1)));
        return (infBalls, weights);
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
                metaball.AddCapsule(segment, function, color: segment.color);
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
                    metaball.AddBall(Mathf.Abs(RandomGaussian(0.5f, 1.5f) * variation) * segment.thickness, position + randomDirection, function, color: segment.color);
                    metaball.AddBall(segment.thickness, position, function, color: segment.color);
                }
            }
        }
        return metaball;
    }

    public static Metaball BuildFromSkeleton(Skeleton skeleton, FalloffFunction function) {
        Metaball metaball = new Metaball();
        foreach (var (go, bone, rb, joint) in skeleton.Iterator()) {
            if (bone.width.HasValue)
            {
                metaball.AddFlattenedCapsule(new(bone.WorldProximalPoint(), bone.WorldDistalPoint(), bone.thickness/2), bone.WorldLateralAxis(), bone.width.Value/2, function, bone: bone, color: bone.color);
            }
            else
            {
                metaball.AddCapsule(new(bone.WorldProximalPoint(), bone.WorldDistalPoint(), bone.thickness), function, bone: bone, color: bone.color);
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
